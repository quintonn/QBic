import { createContext, useContext, useEffect, useState } from "react";
import { API_URL } from "../../Constants/AppValues";
import { AppLayoutProps } from "@cloudscape-design/components";
import { addMessage } from "../../App/flashbarSlice";
import { store } from "../../App/store";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
  DateFormat: string;
}

interface MainAppContextType {
  appName: string;
  appVersion: string;
  dateFormat: string;
  isReady: boolean;
  getCacheValue: (id: string) => any;
  setCacheValue: (id: string, value: any) => void;
  currentContentType: AppLayoutProps.ContentType;
  setCurrentContentType: (value: AppLayoutProps.ContentType) => void;
}

const MainAppContext = createContext<MainAppContextType>(null);

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [dateFormat, setDateFormat] = useState("");
  const [isReady, setIsReady] = useState(false);
  const [cache, setCache] = useState<any>({});
  const [currentContentType, setCurrentContentType] =
    useState<AppLayoutProps.ContentType>("default");

  const initializeSystem = async (): Promise<void> => {
    try {
      const cacheControl = `&_=${Date.now()}`; // don't cache stuff
      const urlToCall = `${API_URL}initializeSystem?v=${cacheControl}`;
      const resp = await fetch(urlToCall);
      const systemInfo = (await resp.json()) as SystemInfo;
      if (systemInfo) {
        if (systemInfo.ConstructionError) {
          console.log("There was an error in the system initialization code:");
          console.log(systemInfo.ConstructionError);

          store.dispatch(
            addMessage({
              type: "error",
              content: `"There was an error in the system initialization code:" ${systemInfo.ConstructionError}`,
            })
          );
        } else {
          setAppName(systemInfo.ApplicationName);
          setAppVersion(systemInfo.Version);
          setDateFormat(systemInfo.DateFormat);

          document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
          setIsReady(true);
        }
      }
    } catch (err) {
      console.error("Fatal error calling initialize system:");
      // wouldn't really happen because back-end hosts front-end
      console.error(err);
      store.dispatch(
        addMessage({
          type: "error",
          content: `Fatal error: ${err}`,
        })
      );
    }
  };

  useEffect(() => {
    initializeSystem();
  }, []);

  const getCacheValue = (id: string): any => {
    const result = cache[id];
    return result;
  };

  const setCacheValue = (id: string, value: any) => {
    setCache({ ...cache, [id]: value });
  };

  const value = {
    appName,
    appVersion,
    dateFormat,
    isReady,
    getCacheValue,
    setCacheValue,
    currentContentType,
    setCurrentContentType,
  };

  return (
    <MainAppContext.Provider value={value}>{children}</MainAppContext.Provider>
  );
};

export const useMainApp = () => {
  const context = useContext(MainAppContext);
  if (!context) {
    throw new Error("useMainApp must be used within an MainAppProvider");
  }
  return context;
};

import { createContext, useContext, useEffect, useState } from "react";
import { API_URL } from "../../Constants/AppValues";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

interface MainAppContextType {
  appName: string;
  appVersion: string;
  isReady: boolean;
}

const MainAppContext = createContext<MainAppContextType>(null);

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [isReady, setIsReady] = useState(false);

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

          //TODO: show error somewhere
          //    : Maybe using the modal proider
        } else {
          setAppName(systemInfo.ApplicationName);
          setAppVersion(systemInfo.Version);

          document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
          setIsReady(true);
        }
      }
    } catch (err) {
      console.error("Fatal error calling initialize system:");
      // wouldn't really happen because back-end hosts front-end
      console.error(err);
      alert("fatal error");
    }
  };

  useEffect(() => {
    console.log("main app hook useEffect");
    initializeSystem();
  }, []);

  const value = {
    appName,
    appVersion,
    isReady,
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

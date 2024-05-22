import { createContext, useContext, useEffect, useState } from "react";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

const MainAppContext = createContext(null);

const API_VERSION = "v1";

const scheme = window.location.protocol;
let _url = scheme + "//" + window.location.host + window.location.pathname;

if (process.env.ROOT_URL) {
  _url = process.env.ROOT_URL;
}

if (!_url.endsWith("/")) {
  _url += "/";
}

const _apiUrl = `${_url}api/${API_VERSION}/`;

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [isReady, setIsReady] = useState(false);

  const initializeSystem = async (): Promise<void> => {
    const cacheControl = `&_=${Date.now()}`; // don't cache stuff
    const urlToCall = `${_apiUrl}initializeSystem?v=${cacheControl}`;
    const resp = await fetch(urlToCall);
    const systemInfo = (await resp.json()) as SystemInfo;
    if (systemInfo) {
      if (systemInfo.ConstructionError) {
        console.log("There was an error in the system initialization code:");
        console.log(systemInfo.ConstructionError);

        //TODO: show error somewhere
      } else {
        setAppName(systemInfo.ApplicationName);
        setAppVersion(systemInfo.Version);

        document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
        setIsReady(true);
      }
    }
  };

  useEffect(() => {
    initializeSystem();
  }, []);

  const value = {
    appName,
    appVersion,
    isReady,
    apiUrl: _apiUrl,
    baseUrl: _url,
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

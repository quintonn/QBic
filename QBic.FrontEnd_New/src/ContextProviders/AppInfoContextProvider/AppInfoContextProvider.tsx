import { createContext, useContext, useEffect, useState } from "react";

export const AppInfoContext = createContext(null);

export const useAppInfo = () => {
  const context = useContext(AppInfoContext);
  if (!context) {
    throw new Error("useAppInfo must be used within an AppInfoContext");
  }
  return context;
};

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

export const AppInfoContextProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");

  const [baseUrl, setBaseUrl] = useState(_url);
  const [apiUrl, setApiUrl] = useState(_apiUrl);

  const value = {
    appName,
    setAppName,
    appVersion,
    setAppVersion,
    baseUrl,
    setBaseUrl,
    apiUrl,
    setApiUrl,
  };
  return (
    <AppInfoContext.Provider value={value}>{children}</AppInfoContext.Provider>
  );
};

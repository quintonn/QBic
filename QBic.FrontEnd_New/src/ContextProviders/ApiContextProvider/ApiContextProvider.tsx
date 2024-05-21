import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "../AuthProvider/AuthContextProvider";
import { useAppInfo } from "../AppInfoContextProvider/AppInfoContextProvider";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

const ApiContext = createContext(null);

const API_VERSION = "v1";

export const ApiContextProvider = ({ children }) => {
  const auth = useAuth();
  const appInfo = useAppInfo();

  //const [apiUrl, setApiUrl] = useState(appInfo.apiUrl);
  //const [appVersion, setAppVersion] = useState(appInfo.appVersion);

  //   const [isReady, setIsReady] = useState(false);

  //   useEffect(() => {
  //     console.log("appInfo changed");
  //     console.log(appInfo);
  //     setApiUrl(appInfo.apiUrl);
  //     setAppVersion(appInfo.appVersion);
  //   }, [appInfo]);

  //   const getUrl = (url: string) => {
  //     let cacheControl = `&_=${Date.now()}`; // don't cache stuff
  //     if (url.includes("html")) {
  //       cacheControl = "";
  //     }

  //     console.log("apiUrl = " + apiUrl);

  //     const urlToCall = `${apiUrl}${url}?v=${appVersion}${cacheControl}`;
  //     return urlToCall;
  //   };

  const makeApiCall = async <T extends any>(
    url: string,
    method: "GET" | "POST" = "GET",
    data?: any,
    raiseErrors: boolean = true
  ): Promise<T> => {
    // Setup
    let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    if (url.includes("html")) {
      cacheControl = "";
    }
    const urlToCall = `${appInfo.apiUrl}${url}?v=${appInfo.appVersion}${cacheControl}`;

    console.log(appInfo);
    console.log("url to call", urlToCall);
    //const urlToCall = getUrl(url);
    console.log("url to call", urlToCall);

    // make API call

    const fetchOptions: RequestInit = {
      method: method,
    };

    if (method == "POST") {
      if (data instanceof FormData) {
        fetchOptions.body = data;
      } else {
        fetchOptions.body = JSON.stringify(data);
      }
    }

    return await makeApiCallInternal(urlToCall, fetchOptions);

    // alert(
    //   "Error contacting the server. You might not have internet or our server is down"
    // );
  };

  const initializeSystem = async (): Promise<SystemInfo> => {
    // console.log('setting initialize system');
    // console.log(appInfo);
    // const scheme = window.location.protocol; //"https";
    // let _url = scheme + "//" + window.location.host + window.location.pathname;
    // //console.log("base url = " + baseURL);
    // //console.log("root url = " + process.env.ROOT_URL);

    // if (process.env.ROOT_URL) {
    //   //console.log("dev root url is not empty");
    //   _url = process.env.ROOT_URL;
    // }

    // if (!_url.endsWith("/")) {
    //   _url += "/";
    // }

    // console.log("setting base url to ", _url);
    // appInfo.setBaseUrl(_url);

    // const _apiUrl = `${_url}api/${API_VERSION}/`;
    // appInfo.setApiUrl(_apiUrl);

    // TODO: Move all of the above to a hook or something (maybe just a file that gets imported by app.ts);

    //const url = `initializeSystem`;

    let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    const urlToCall = `${appInfo.apiUrl}initializeSystem?v=${appInfo.appVersion}${cacheControl}`;

    const fetchOptions: RequestInit = {
      method: "GET",
    };

    const systemInfo = await makeApiCallInternal<SystemInfo>(
      urlToCall,
      fetchOptions
    );
    if (systemInfo) {
      if (systemInfo.ConstructionError) {
        console.log("There was an error in the system initialization code:");
        console.log(systemInfo.ConstructionError);
        return null;
      }
      //appInfo.setAppVersion(systemInfo.Version);
      document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
    }

    return systemInfo;
  };

  const makeApiCallInternal = async <T extends any>(
    urlToCall: string,
    fetchOptions: RequestInit,
    raiseErrors: boolean = true
  ): Promise<T> => {
    // make API call
    try {
      //console.trace("making api call", urlToCall);
      const response = await fetch(urlToCall, fetchOptions);

      if (response.status >= 200 && response.status < 300) {
        // success
        var json = (await response.json()) as T;
        return Promise.resolve(json);
      } else if (response.status == 401) {
        console.log("unauthorized, calling refresh token now");
        console.log(appInfo);
        await auth.performTokenRefresh(); //TODO: Api service will have to be a hook to so we can use this part
        // try freshing the token and re-doing the api call
      } else {
        console.log("unhanled response status: ", response.status);
        //alert("Unhandled response status: " + response.status);
        return null;
        //TODO: handle response
      }
    } catch (err) {
      console.log("error making api call to: " + urlToCall);
      console.log(err);
      if (raiseErrors == true) {
        //TODO: show these errors somewhere (flashbar);
      } else {
        return null;
      }

      // alert(
      //   "Error contacting the server. You might not have internet or our server is down"
      // );
    }
  };

  const value = { makeApiCall, initializeSystem };

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>;
};

export const useApi = () => {
  const context = useContext(ApiContext);
  if (!context) {
    throw new Error("useApi must be used within an ApiContextProvider");
  }
  return context;
};

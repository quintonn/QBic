import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "../AuthProvider/AuthContextProvider";
import { useAppInfo } from "../AppInfoContextProvider/AppInfoContextProvider";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

const ApiContext = createContext(null);

export const ApiContextProvider = ({ children }) => {
  const auth = useAuth();
  const appInfo = useAppInfo();

  console.log("inside api context provider");

  const makeApiCall = async <T extends any>(
    url: string,
    method: "GET" | "POST" = "GET",
    data?: any
  ): Promise<T> => {
    // Setup
    let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    if (url.includes("html")) {
      cacheControl = "";
    }
    const urlToCall = `${appInfo.apiUrl}${url}?v=${appInfo.appVersion}${cacheControl}`;

    // make API call
    const fetchOptions: RequestInit = {
      method: method,
      headers: {
        Authorization: "Bearer " + auth.accessToken,
      },
    };

    console.log("fetch Options");
    console.log(fetchOptions);

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
    // let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    // const urlToCall = `${appInfo.apiUrl}initializeSystem?v=${appInfo.appVersion}${cacheControl}`;

    // const fetchOptions: RequestInit = {
    //   method: "GET",
    // };

    const systemInfo = await makeApiCall<SystemInfo>("initializeSystem");
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
      const response = await fetch(urlToCall, fetchOptions);

      if (response.status >= 200 && response.status < 300) {
        // success
        var json = (await response.json()) as T;
        return Promise.resolve(json);
      } else if (response.status == 401) {
        console.log("unauthorized, calling refresh token now");
        return auth
          .performTokenRefresh()
          .then((x) => {
            // try call again
            console.log("refresh token updated successfully");
            return makeApiCallInternal(
              urlToCall,
              fetchOptions,
              raiseErrors
            ).then((x) => {
              return Promise.resolve(x);
            });
          })
          .catch((err) => {
            console.error("error while getting refresh token");
            // TODO: show login dialog -> which should then essentially restart the application initialization stuff as it will have new tokens
            auth.doLogin();
          });
      } else if (response.status == 400) {
        const text = await response.text();
        if (text.includes("invalid_grant")) {
          console.log("username or password incorrect");
          // show message that username or password was incorrect
        }
      }

      console.log("unhanled response status: ", response.status);
      return null;
      //TODO: handle response
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

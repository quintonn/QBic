import { API_URL } from "../Constants/AppValues";
import { useAuth } from "../ContextProviders/AuthProvider/AuthProvider";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

export const useApi = () => {
  const auth = useAuth();

  const { appVersion } = useMainApp();

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
    const urlToCall = `${API_URL}${url}?v=${appVersion}${cacheControl}`;

    // make API call
    const fetchOptions: RequestInit = {
      method: method,
      headers: {
        Authorization: "Bearer " + auth.accessToken,
      },
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
      if (response.ok) {
        // success
        var json = (await response.json()) as T;
        return Promise.resolve(json);
      } else if (response.status == 401) {
        if (auth.refreshToken) {
          return auth
            .performTokenRefresh()
            .then((x) => {
              // try call again

              return makeApiCallInternal<T>(
                urlToCall,
                fetchOptions,
                raiseErrors
              ).then((x) => {
                return Promise.resolve(x as T);
              });
            })
            .catch((err) => {
              console.error("error while getting refresh token");
              // TODO: show login dialog -> which should then essentially restart the application initialization stuff as it will have new tokens
              //auth.doLogin();
              // setShowLoginDialog -> or something like that
              return Promise.resolve(null as T);
            });
        } else {
          return Promise.reject(401);
        }
      } else if (response.status == 400) {
        const text = await response.text();
        if (text.includes("invalid_grant")) {
          console.log("username or password incorrect");
          // show message that username or password was incorrect
        }
      }

      console.log("unhanled response status: ", response.status);
      return Promise.reject("Error making api call: " + response.status);
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

  return { makeApiCall, initializeSystem };
};

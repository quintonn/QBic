import { useEffect, useState } from "react";
import { SystemInfo } from "./apiHook";

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

const cacheControl = `&_=${Date.now()}`; // don't cache stuff
const urlToCall = `${_apiUrl}initializeSystem?v=${cacheControl}`;

export const useMainAppxx = () => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [isReady, setIsReady] = useState(false);

  const initializeSystem = async (): Promise<void> => {
    //console.log("main hook calling initialize system");
    const resp = await fetch(urlToCall);
    const systemInfo = (await resp.json()) as SystemInfo;
    if (systemInfo) {
      if (systemInfo.ConstructionError) {
        console.log("There was an error in the system initialization code:");
        console.log(systemInfo.ConstructionError);

        //TODO: raise a big error
      } else {
        setAppName(systemInfo.ApplicationName);
        setAppVersion(systemInfo.Version);

        document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
        setTimeout(() => {
          setIsReady(true);
        }, 5000);
      }
    }
  };

  useEffect(() => {
    console.log("useEffect of main App Hook");
    //initializeSystem();
  }, []);

  return { appName, appVersion, isReady, apiUrl: _apiUrl, baseUrl: _url };
};

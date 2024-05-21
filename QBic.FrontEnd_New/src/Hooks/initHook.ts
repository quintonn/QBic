import { useEffect, useState } from "react";

import { useAuth } from "../ContextProviders/AuthProvider/AuthContextProvider";
import { useApi } from "../ContextProviders/ApiContextProvider/ApiContextProvider";
import { useAppInfo } from "../ContextProviders/AppInfoContextProvider/AppInfoContextProvider";

interface UserInfo {
  User: string;
  Id: string;
}

export const useInit = () => {
  const [loading, setLoading] = useState(true);
  //const [systemInfo, setSystemInfo] = useState<SystemInfo>(null);
  const [userInfo, setUserInfo] = useState<UserInfo>(null);
  const appInfo = useAppInfo();
  const { initializeAuth, isReady: authIsReady } = useAuth();

  const { initializeSystem, makeApiCall } = useApi();

  const [isReady, setIsReady] = useState(false);

  //const { appMenuItems, sideNavMenuItems } = useMenus(); //TODO: Menus should only be fetched if user is logged in!

  // need to do our own auth ??

  useEffect(() => {
    async function onReadyFunction() {
      // then initialize auth system -> basically checks local storage for auth tokens and gets new tokens if needs to
      console.log("initializing auth");
      await initializeAuth();
    }

    if (isReady === true) {
      console.log("is ready now fired");
      console.log(appInfo);
      onReadyFunction();
    }

    // then load user menus

    // If the initialize fails, we need to check if an anonymous function is called, because this is how password reset works
  }, [isReady]);

  useEffect(() => {
    async function onReadyFunction() {
      // call initialize (basically checks if user is authenticated, and returns user name and id)
      console.log("calling initialize...");
      const _userInfo = await makeApiCall("initialize");
      console.log(_userInfo);
    }

    console.log("authIsReady changed", authIsReady);

    if (authIsReady === true) {
      setTimeout(() => {
        console.log("XXXXXXXXXXXXXXXXXX");
        onReadyFunction();
      }, 3000);
    }

    // then load user menus

    // If the initialize fails, we need to check if an anonymous function is called, because this is how password reset works
  }, [authIsReady]);

  useEffect(() => {
    async function initializeApp() {
      try {
        const systemInfo = await initializeSystem();
        console.log("get system info", systemInfo);
        console.log("setting appInfo appname", systemInfo.ApplicationName);
        appInfo.setAppName(systemInfo.ApplicationName);
        appInfo.setAppVersion(systemInfo.Version);

        setTimeout(() => {
          console.log("setting is ready 1");
          setIsReady(true);
        }, 3000);
      } catch (err) {
        console.log("error during initialization");
        console.error(err);
        //TODO: show a message to user
      }
    }

    initializeApp();
  }, []);

  return {
    loading,
    userInfo,
    //menus: { appMenuItems, sideNavMenuItems },
  };
};

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
    async function initializeApp() {
      try {
        const systemInfo = await initializeSystem();

        appInfo.setAppName(systemInfo.ApplicationName);
        appInfo.setAppVersion(systemInfo.Version);

        setIsReady(true);
      } catch (err) {
        console.log("error during initialization");
        console.error(err);
        //TODO: show a message to user
      }
    }

    initializeApp();
  }, []);

  useEffect(() => {
    async function onReadyFunction() {
      await initializeAuth();
    }

    if (isReady === true) {
      onReadyFunction();
    }
  }, [isReady]);

  useEffect(() => {
    async function onReadyFunction() {
      // call initialize (basically checks if user is authenticated, and returns user name and id)

      const _userInfo = await makeApiCall("initialize");
      console.log(_userInfo);
    }

    if (authIsReady === true) {
      onReadyFunction();
    }
  }, [authIsReady]);

  // TODO: Still do the following stuff
  // then load user menus
  // If the initialize fails, we need to check if an anonymous function is called, because this is how password reset works

  return {
    loading,
    userInfo,
    //menus: { appMenuItems, sideNavMenuItems },
  };
};

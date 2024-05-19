import { useEffect, useState } from "react";
import { initializeSystem, makeApiCall } from "../Services/apiService";
import { useAppInfo } from "./appInfoHook";
import { useAuth } from "./authHook";

interface UserInfo {
  User: string;
  Id: string;
}

export const useInit = () => {
  const [loading, setLoading] = useState(true);
  //const [systemInfo, setSystemInfo] = useState<SystemInfo>(null);
  const [userInfo, setUserInfo] = useState<UserInfo>(null);
  const appInfo = useAppInfo();
  const { initializeAuth } = useAuth();

  //const { appMenuItems, sideNavMenuItems } = useMenus(); //TODO: Menus should only be fetched if user is logged in!

  // need to do our own auth ??

  useEffect(() => {
    async function initializeApp() {
      try {
        const systemInfo = await initializeSystem();
        console.log("get system info", systemInfo);
        appInfo.setAppName(systemInfo.ApplicationName);
        appInfo.setAppVersion(systemInfo.Version);

        // then initialize auth system -> basically checks local storage for auth tokens and gets new tokens if needs to
        console.log("initializing auth");
        await initializeAuth();

        // call initialize (basically checks if user is authenticated, and returns user name and id)
        const _userInfo = await makeApiCall<UserInfo>("initialize");
        console.log(_userInfo);

        // then load user menus

        // If the initialize fails, we need to check if an anonymous function is called, because this is how password reset works
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

import { useEffect, useState } from "react";
import { useMenus } from "./menuHook";
import {
  SystemInfo,
  initializeSystem,
  makeApiCall,
} from "../Services/apiService";

interface UserInfo {
  User: string;
  Id: string;
}

export const useInit = () => {
  const [loading, setLoading] = useState(true);
  const [systemInfo, setSystemInfo] = useState<SystemInfo>(null);
  const [userInfo, setUserInfo] = useState<UserInfo>(null);

  //const { appMenuItems, sideNavMenuItems } = useMenus(); //TODO: Menus should only be fetched if user is logged in!

  // need to do our own auth ??

  useEffect(() => {
    async function doStuff() {
      const _systemInfo = await initializeSystem();
      setSystemInfo(_systemInfo);

      // then initialize auth system -> basically checks local storage for auth tokens and gets new tokens if needs to

      // call initialize (basically checks if user is authenticated, and returns user name and id)
      const _userInfo = await makeApiCall<UserInfo>("initialize");
      console.log(_userInfo);

      // then load user menus

      // If the initialize fails, we need to check if an anonymous function is called, because this is how password reset works
    }

    doStuff();
  }, []);

  return {
    loading,
    systemInfo,
    userInfo,
    //menus: { appMenuItems, sideNavMenuItems },
  };
};

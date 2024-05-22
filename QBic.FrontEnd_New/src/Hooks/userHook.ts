import { useEffect } from "react";
import { useAuth } from "./authHook";
import { useApi } from "./apiHook";

export const useUser = () => {
  const tmp = "";

  const { isReady: authIsReady } = useAuth();
  const { makeApiCall } = useApi();

  useEffect(() => {
    if (authIsReady === true) {
      //onReadyFunction();
    }
  }, [authIsReady]);

  async function onReadyFunction() {
    // call initialize (basically checks if user is authenticated, and returns user name and id)

    console.log("on ready function calling initialize");
    const _userInfo = await makeApiCall("initialize", "GET");
    console.log(_userInfo);
  }

  return { tmp };
};

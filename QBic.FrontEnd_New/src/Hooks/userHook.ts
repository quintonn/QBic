import { useEffect, useState } from "react";
import { useAuth } from "./authHook";
import { useApi } from "./apiHook";
import { useStartup } from "./startupHook";
import { useNavigate } from "react-router-dom";

export const useUser = () => {
  const { isReady: startupIsReady } = useStartup();
  const { makeApiCall } = useApi();
  const auth = useAuth();

  const [isReady, setIsReady] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    if (startupIsReady === true) {
      console.log("startupIsReady is ready");
      onReadyFunction();
    }
  }, [startupIsReady]);

  async function onReadyFunction() {
    // call initialize (basically checks if user is authenticated, and returns user name and id)

    console.log("on ready function calling initialize");
    makeApiCall("initialize", "GET")
      .then((userInfo) => {
        console.log(userInfo);
        setIsReady(true);
      })
      .catch((err) => {
        if (err === 401) {
          // initialize call failed (it should have tried to refresh the token if it had one)
          // do login

          console.log("show login screen");

          // var temp = confirm("Auto login?");
          // if (temp === true) {
          //   let username = "admin";
          //   let password = "password";
          //   auth.doLogin(username, password);
          // }
          navigate("/login");
        }
      });
  }

  return { isReady };
};

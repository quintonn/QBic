import { useEffect, useState } from "react";
import { useAuth } from "./authHook";

export const useStartup = () => {
  const { isReady: authIsReady } = useAuth();
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    if (authIsReady === true) {
      // check anon function etc.
      setIsReady(true);
    }
  }, [authIsReady]);

  return { isReady };
};

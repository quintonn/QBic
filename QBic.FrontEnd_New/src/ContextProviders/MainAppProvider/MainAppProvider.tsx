import { createContext, useContext, useEffect, useState } from "react";
import { API_URL } from "../../Constants/AppValues";
import { AppLayoutProps } from "@cloudscape-design/components";
import { addMessage } from "../../App/flashbarSlice";
import { store } from "../../App/store";

import { v4 as uuidv4 } from "uuid";
import { MenuDetail } from "../MenuProvider/MenuProvider";
import { Login } from "../../Components/Login/Login.component";
import { FormComponent } from "../../Components/Form/Form.component";
import { ViewComponent } from "../../Components/View/View.component";
import { Home } from "../../Components/Home/Home.component";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
  DateFormat: string;
}

type displayType = "form" | "view" | "home" | "login";

export interface DisplayItem extends AbbrDisplayItem {
  visible: boolean;
  component: () => React.ReactNode;
  id: string;
}

export interface AbbrDisplayItem {
  menu?: MenuDetail;
  type: displayType;
}

interface MainAppContextType {
  appName: string;
  appVersion: string;
  dateFormat: string;
  isReady: boolean;
  getCacheValue: (id: string) => any;
  setCacheValue: (id: string, value: any) => void;
  currentContentType: AppLayoutProps.ContentType;
  setCurrentContentType: (value: AppLayoutProps.ContentType) => void;
  updateFormCacheStack: () => string | null;
  popFormCache: () => string | null;
  getUseCachedValues: (formId: number) => boolean;
  updateUseCachedValues: (formId: number, value: boolean) => void;
  inputViewUpdateData: any;
  setInputViewUpdateData: (data: any) => void;
  currentItem: DisplayItem;
  setCurrentItem: (value: AbbrDisplayItem | -1) => void;
  displayStack: DisplayItem[];
  clearDisplayStack: () => void;
}

const MainAppContext = createContext<MainAppContextType>(null);

//const DisplayStack: DisplayItem[] = [];

const homeDisplayItem: DisplayItem = {
  menu: null,
  type: "home",
  visible: true,
  id: "home",
  component: () => <Home />,
};

let clearingStack = false;

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [dateFormat, setDateFormat] = useState("");
  const [isReady, setIsReady] = useState(false);
  const [cache, setCache] = useState<any>({});
  const [currentContentType, setCurrentContentType] =
    useState<AppLayoutProps.ContentType>("default");

  const [currentItem, updateCurrentItem] =
    useState<DisplayItem>(homeDisplayItem);

  const [displayStack, setDisplayStack] = useState<DisplayItem[]>([
    homeDisplayItem,
  ]);

  const makeLastItemVisible = (stack: DisplayItem[]) => {
    if (stack.length > 0) {
      stack[stack.length - 1].visible = true;
    }
  };

  const hideAllItems = (stack: DisplayItem[]) => {
    for (let i = 0; i < stack.length; i++) {
      stack[i].visible = false;
    }

    // const updatedItems = stack.map((item) => ({
    //   ...item,
    //   visible: false,
    // }));
  };

  const getComponent = (display: DisplayItem) => {
    switch (display.type) {
      case "login":
        return <Login />;
      case "form":
        return (
          <FormComponent menuItem={display.menu} visible={display.visible} />
        );
      case "view":
        return <ViewComponent menuItem={display.menu} />;
      case "home":
        return <Home />;
    }
  };

  const addDisplayItem = (
    item: AbbrDisplayItem,
    visible: boolean,
    stack: DisplayItem[]
  ): DisplayItem => {
    const stackItem: DisplayItem = {
      component: () => null,
      id: uuidv4(),
      visible: visible,
      menu: item.menu,
      type: item.type,
    };
    stackItem.component = () => {
      return getComponent(stackItem);
    };
    stack.push(stackItem);
    return stack[stack.length - 1];
  };

  const setCurrentItem = (displayItem: AbbrDisplayItem | -1) => {
    let stack = [...displayStack];
    hideAllItems(stack);

    if (clearingStack === true) {
      clearingStack = false;
      stack = [homeDisplayItem];
    }

    if (displayItem == -1) {
      // remove current item
      if (stack.length > 0) {
        const index = stack.length - 1;
        const lastItem = stack[index];
        stack = [...stack.slice(0, index), ...stack.slice(index + 1)];
      }
      if (displayStack.length == 0) {
        const item = addDisplayItem(homeDisplayItem, true, stack);
        updateCurrentItem(item);
      } else {
        updateCurrentItem(displayStack[displayStack.length - 1]);
      }
    } else {
      //displayStack.push(displayItem);
      const item = addDisplayItem(displayItem, true, stack);
      updateCurrentItem(item); // to make sure the current item matches the added one
    }

    makeLastItemVisible(stack);

    setDisplayStack(stack); // TODO: this seems to work, but make all other items not visible and clean up code
  };

  const [inputViewUpdateData, setInputViewUpdateData] = useState<any>(null);

  const [formCachStack, setFormCacheStack] = useState<string[]>([]);
  const [useCachedValues, setUseCachedValues] = useState<
    Record<number, boolean>
  >({});

  const initializeSystem = async (): Promise<void> => {
    try {
      const cacheControl = `&_=${Date.now()}`; // don't cache stuff
      const urlToCall = `${API_URL}initializeSystem?v=${cacheControl}`;
      const resp = await fetch(urlToCall);
      const systemInfo = (await resp.json()) as SystemInfo;
      if (systemInfo) {
        if (systemInfo.ConstructionError) {
          console.log("There was an error in the system initialization code:");
          console.log(systemInfo.ConstructionError);

          store.dispatch(
            addMessage({
              type: "error",
              content: `"There was an error in the system initialization code:" ${systemInfo.ConstructionError}`,
            })
          );
        } else {
          setAppName(systemInfo.ApplicationName);
          setAppVersion(systemInfo.Version);
          setDateFormat(systemInfo.DateFormat);

          document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
          setIsReady(true);
        }
      }
    } catch (err) {
      console.error("Fatal error calling initialize system:");
      // wouldn't really happen because back-end hosts front-end
      console.error(err);
      store.dispatch(
        addMessage({
          type: "error",
          content: `Fatal error: ${err}`,
        })
      );
    }
  };

  useEffect(() => {
    initializeSystem();
  }, []);

  const getCacheValue = (id: string): any => {
    const result = cache[id];
    return result;
  };

  const setCacheValue = (id: string, value: any) => {
    setCache({ ...cache, [id]: value });
  };

  const getFormCacheStack = (): string | null => {
    if (formCachStack.length > 0) {
      return formCachStack[formCachStack.length - 1];
    }
    return null;
  };

  const updateFormCacheStack = (): string => {
    const id = uuidv4();
    setFormCacheStack([...formCachStack, id]);
    return id;
  };

  const getUseCachedValues = (id: number) => {
    return useCachedValues[id];
  };

  const updateUseCachedValues = (id: number, value: boolean) => {
    setUseCachedValues((prevValues) => ({
      ...prevValues,
      [id]: value,
    }));
  };

  const popFormCache = () => {
    const lastValue = getFormCacheStack();
    setFormCacheStack((prevStack) => prevStack.slice(0, prevStack.length - 1));
    return lastValue;
  };

  const clearDisplayStack = () => {
    clearingStack = true;
    const stack = [];
    const item = addDisplayItem(homeDisplayItem, true, stack);
    updateCurrentItem(item); // to make sure the current item matches the added one
    setDisplayStack([]);
  };

  const value = {
    appName,
    appVersion,
    dateFormat,
    isReady,
    getCacheValue,
    setCacheValue,
    currentContentType,
    setCurrentContentType,
    updateFormCacheStack,
    popFormCache,
    getUseCachedValues,
    updateUseCachedValues,
    inputViewUpdateData,
    setInputViewUpdateData,
    currentItem,
    setCurrentItem,
    displayStack,
    clearDisplayStack,
  };

  return (
    <MainAppContext.Provider value={value}>{children}</MainAppContext.Provider>
  );
};

export const useMainApp = () => {
  const context = useContext(MainAppContext);
  if (!context) {
    throw new Error("useMainApp must be used within an MainAppProvider");
  }
  return context;
};

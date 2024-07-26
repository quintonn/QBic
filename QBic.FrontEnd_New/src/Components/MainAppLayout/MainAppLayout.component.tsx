import {
  AppLayout,
  AppLayoutProps,
  Flashbar,
  SideNavigation,
  SplitPanel,
  TopNavigation,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useAuth } from "../../ContextProviders/AuthProvider/AuthProvider";
import {
  AppMenuItem,
  useMenu,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { removeMessage, selectedMessages } from "../../App/flashbarSlice";
import { useAppDispatch, useAppSelector } from "../../App/hooks";
import { useActions } from "../../ContextProviders/ActionProvider/ActionProvider";
import { SplitPanelComponent } from "../SplitPanelComponent/SplitPanelComponent";

interface MainAppLayoutProps extends AppLayoutProps {}

const findClickedItem = (id: string, items: AppMenuItem[]): AppMenuItem => {
  for (const item of items) {
    if (item.id === id) {
      return item;
    }
    if (item.subMenus) {
      const foundItem = findClickedItem(id, item.subMenus);
      if (foundItem) {
        return foundItem;
      }
    }
  }
  return null;
};

export const MainAppLayout = (_props: MainAppLayoutProps) => {
  const [activeHref, setActiveHref] = useState("/");

  const { messages } = useAppSelector(selectedMessages);
  const dispatch = useAppDispatch();

  const menus = useMenu();
  const { onMenuClick } = useActions();
  const [splitPanelOpen, setSplitPanelOpen] = useState(false);

  const mainApp = useMainApp(); // auto injected because it's a context provider

  const auth = useAuth();

  // TODO: make these provider things too

  // TODO: - it might be easier to wait for main app to be ready, and call all these things manually
  //       - then I can call them as "startApplication" same as before

  // flow:
  // call 'initialize system' to get system information   -> only happens once ever

  // then (a.k.a. startApplication)
  // then auth initialize
  //    -> get local stored stuff
  //    -> call perform token refresh is needed
  // then main app initialize
  //    -> check anon actions
  //    -> call 'initialize'
  //    -> if web call fails with 401
  //           --> call perform token refresh
  //           --> if this errors show login dialog
  //           --> if login dialog succeeds, start this flow again
  // then load menus
  //    -> if this errors check for anon functions

  const handleMenuClick = (itemRef: string) => {
    mainApp.setSelectedTableRow(null);
    if (itemRef == "/") {
      menus.onHomeClick();
      return;
    }
    const itemId = itemRef.substring(1);

    const menuItemClicked = findClickedItem(itemId, menus.appMenuItems);

    mainApp.clearDisplayStack();

    onMenuClick(menuItemClicked.event);
  };

  const clearAllViewFilterCaches = () => {
    // Get the number of items in local storage
    const localStorageLength = localStorage.length;

    // Remove keys that match filter cache
    for (let i = 0; i < localStorageLength; i++) {
      const key = localStorage.key(i);
      if (
        key &&
        (key.includes("_filter_cache") || key.includes("_form_values_cache"))
      ) {
        localStorage.removeItem(key);
      }
    }
  };

  useEffect(() => {
    if (mainApp?.selectedRow) {
      setSplitPanelOpen(true);
    }
  }, [mainApp.selectedRow]);

  const currentDisplayItem = mainApp.displayStack.filter(
    (d) => d.visible == true
  )?.[0]; // this seems to work

  return (
    <>
      <div id="h" style={{ position: "sticky", top: 0, zIndex: 1002 }}>
        <TopNavigation
          identity={{
            href: "/",
            title: mainApp.appName || "QBic",
          }}
          utilities={
            auth.isAuthenticated
              ? [
                  { type: "button", text: auth?.user?.User },
                  { type: "button", text: "Logout", onClick: auth.logout },
                ]
              : []
          }
        />
      </div>
      <AppLayout
        headerSelector="#h"
        splitPanelOpen={splitPanelOpen}
        onSplitPanelToggle={() => setSplitPanelOpen(!splitPanelOpen)}
        splitPanel={
          currentDisplayItem.type == "view" ? (
            <SplitPanelComponent
              showContent={currentDisplayItem.type == "view"}
            />
          ) : null
        } //todo: will have to set/get via mainApp, maybe from display stack item?
        content={currentDisplayItem.component}
        contentType={mainApp.currentContentType}
        toolsHide={true}
        navigationHide={!auth.isAuthenticated}
        navigation={
          <SideNavigation
            activeHref={activeHref}
            onFollow={(event) => {
              event.preventDefault();

              clearAllViewFilterCaches();
              setActiveHref(event.detail.href);
              handleMenuClick(event.detail.href);
            }}
            items={menus.sideNavMenuItems}
          />
        }
        notifications={
          <>
            <Flashbar
              items={messages.map((message) => {
                return {
                  ...message,
                  onDismiss: () => {
                    if (message.id) {
                      dispatch(removeMessage(message.id));
                    }
                  },
                };
              })}
              stackItems
            />
          </>
        }
      ></AppLayout>
    </>
  );
};

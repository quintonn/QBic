import {
  AppLayout,
  AppLayoutProps,
  Flashbar,
  SideNavigation,
  TopNavigation,
} from "@cloudscape-design/components";
import { useState } from "react";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useAuth } from "../../ContextProviders/AuthProvider/AuthProvider";
import {
  AppMenuItem,
  useMenu,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { useActions } from "../../Hooks/actionHook";
import { removeMessage, selectedMessages } from "../../App/flashbarSlice";
import { useAppDispatch, useAppSelector } from "../../App/hooks";

interface MainAppLayoutProps extends AppLayoutProps {
  content: React.ReactNode;
}

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

export const MainAppLayout = ({ content }: MainAppLayoutProps) => {
  const [activeHref, setActiveHref] = useState("#");

  const { messages } = useAppSelector(selectedMessages);
  const dispatch = useAppDispatch();

  const menus = useMenu();
  const { onMenuClick } = useActions();

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
    if (itemRef == "#") {
      menus.onHomeClick();
      return;
    }
    const itemId = itemRef.substring(1);

    const menuItemClicked = findClickedItem(itemId, menus.appMenuItems);

    onMenuClick(menuItemClicked.event);
  };

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
        content={content}
        contentType={mainApp.currentContentType}
        toolsHide={true}
        navigationHide={!auth.isAuthenticated}
        navigation={
          <SideNavigation
            activeHref={activeHref}
            onFollow={(event) => {
              event.preventDefault();
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

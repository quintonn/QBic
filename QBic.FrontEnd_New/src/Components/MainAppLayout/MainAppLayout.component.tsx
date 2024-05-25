import {
  AppLayout,
  AppLayoutProps,
  SideNavigation,
  TopNavigation,
} from "@cloudscape-design/components";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { AppMenuItem, useMenus } from "../../Hooks/menuHook";
import { useApi } from "../../Hooks/apiHook";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useAuth } from "../../ContextProviders/AuthProvider/AuthProvider";

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
  const navigate = useNavigate();

  const { appMenuItems, sideNavMenuItems } = useMenus();

  const mainApp = useMainApp(); // auto injected because it's a context provider

  const auth = useAuth();
  const api = useApi(); // calls auth -> calls main App

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
      console.log("home clicked");
      navigate("/");
      return;
    }
    const itemId = itemRef.substring(1);

    const menuItemClicked = findClickedItem(itemId, appMenuItems);

    console.log(
      "TODO: Handle menu item event clicked: " + menuItemClicked.event
    );
    //TODO: handle menuItemClicked.event
  };

  return (
    <>
      {/* {!user.isReady && <Login />} */}
      <div id="h" style={{ position: "sticky", top: 0, zIndex: 1002 }}>
        <TopNavigation
          // i18nStrings={i18nStrings}
          identity={{
            href: "/",
            title: mainApp.appName || "QBic",
            //   logo: { src: logo, alt: "Service name logo" },
          }}
          utilities={
            auth.isAuthenticated
              ? [{ type: "button", text: "Logout", onClick: auth.logout }]
              : []
            // : [{ type: "button", text: "Sign In", href: PATHS.signin.path }]
          }
          // search={
          //   <Input
          //     ariaLabel="Input field"
          //     clearAriaLabel="Clear"
          //     value={searchValue}
          //     type="search"
          //     placeholder="Search"
          //     onChange={({ detail }) => setSearchValue(detail.value)}
          //   />
          // }
          // utilities={[
          //   {
          //     type: "button",
          //     iconName: "notification",
          //     ariaLabel: "Notifications",
          //     badge: true,
          //     disableUtilityCollapse: true,
          //   },
          //   {
          //     type: "button",
          //     iconName: "settings",
          //     title: "Settings",
          //     ariaLabel: "Settings",
          //   },
          //   {
          //     type: "menu-dropdown",
          //     text: "Customer name",
          //     description: "customer@example.com",
          //     iconName: "user-profile",
          //     items: profileActions,
          //   },
          //]}
        />
      </div>
      <AppLayout
        headerSelector="#h"
        content={content}
        toolsHide={true}
        navigationHide={!auth.isAuthenticated}
        navigation={
          <SideNavigation
            activeHref={activeHref}
            // header={{ href: currentMenuItem.href, text: currentMenuItem.name }}
            onFollow={(event) => {
              event.preventDefault();
              setActiveHref(event.detail.href);
              handleMenuClick(event.detail.href);
            }}
            items={sideNavMenuItems}
          />
        }
      ></AppLayout>
    </>
  );
};

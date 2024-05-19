import {
  AppLayout,
  AppLayoutProps,
  SideNavigation,
  TopNavigation,
} from "@cloudscape-design/components";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { AppMenuItem, useMenus } from "../../Hooks/menuHook";
import { useInit } from "../../Hooks/initHook";

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

  const initInfo = useInit();

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
      <div id="h" style={{ position: "sticky", top: 0, zIndex: 1002 }}>
        <TopNavigation
          // i18nStrings={i18nStrings}
          identity={{
            href: "/",
            title: "App name",
            //   logo: { src: logo, alt: "Service name logo" },
          }}
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

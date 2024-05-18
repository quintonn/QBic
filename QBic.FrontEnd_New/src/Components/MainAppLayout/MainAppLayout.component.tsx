import {
  AppLayout,
  AppLayoutProps,
  SideNavigation,
  SideNavigationProps,
  TopNavigation,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import { MenuItem, TestMenuData } from "../../TestData/Menus";
import { useLocation, useNavigate } from "react-router-dom";

interface MainAppLayoutProps extends AppLayoutProps {
  content: React.ReactNode;
}

interface AppMenuItem {
  href: string;
  path: string;
  name: string;
  parentMenu: any;
  subMenus: AppMenuItem[] | null;
  event: number | null;
  position: number;
  id: string;
  canDelete: boolean;
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
const MapMenuItemsToSideNavItems = (
  items: MenuItem[],
  root: boolean = true
): SideNavigationProps.Item[] => {
  const results: SideNavigationProps.Item[] = [];

  if (root) {
    results.push({ text: "Home", href: "#", type: "link" });
  }

  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    if (item.SubMenus && item.SubMenus.length > 0) {
      const sectionItem = {
        text: item.Name,
        type: "section",
        items: MapMenuItemsToSideNavItems(item.SubMenus, false),
      } as SideNavigationProps.Item;

      results.push(sectionItem);
    } else {
      results.push({ text: item.Name, href: "#" + item.Id, type: "link" });
    }
  }

  return results;
};

const MapMenuItemToAppMenuItem = (
  items: MenuItem[],
  path: string = ""
): AppMenuItem[] => {
  if (!items) {
    return [];
  }
  return items.map((item) => ({
    path: path + "/" + item.Id,
    href: item.Id,
    id: item.Id,
    name: item.Name,
    parentMenu: null,
    subMenus: [
      {
        id: "#back",
        event: null,
        position: 0,
        canDelete: false,
        href: "#back/" + item.Id,
        name: "<<",
        parentMenu: null,
        path: "",
        subMenus: [],
      },
      ...MapMenuItemToAppMenuItem(item.SubMenus, path + "/" + item.Id),
    ],
    event: item.Event,
    position: item.Position,
    canDelete: item.CanDelete,
  }));
};

const allItems = MapMenuItemToAppMenuItem(TestMenuData as MenuItem[], ""); //TODO: fetch this async etc.

const sideNavItems = MapMenuItemsToSideNavItems(TestMenuData as MenuItem[]);

export const MainAppLayout = ({ content }: MainAppLayoutProps) => {
  const [activeHref, setActiveHref] = useState("#");
  const navigate = useNavigate();

  const handleMenuClick = (itemRef: string) => {
    if (itemRef == "#") {
      console.log("home clicked");
      navigate("/");
      return;
    }
    const itemId = itemRef.substring(1);

    const menuItemClicked = findClickedItem(itemId, allItems);

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
            items={sideNavItems}
          />
        }
      ></AppLayout>{" "}
    </>
  );
};

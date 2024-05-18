export interface MenuItem {
  Name: string;
  ParentMenu: null;
  SubMenus: MenuItem[] | null;
  Event: number | null;
  Position: number;
  Id: string;
  CanDelete: boolean;
}

export const TestMenuData = [
  {
    Name: "System",
    ParentMenu: null,
    SubMenus: [
      {
        Name: "Users",
        ParentMenu: null,
        SubMenus: [],
        Event: 1000,
        Position: 0,
        Id: "97eccb68-f3d0-4dbf-b624-0589cdb41e74",
        CanDelete: true,
      },
      {
        Name: "Menus",
        ParentMenu: null,
        SubMenus: [],
        Event: 1007,
        Position: 1,
        Id: "c1c4f7c2-d6fb-4777-ae20-9df545cb4c3c",
        CanDelete: true,
      },
      {
        Name: "User Roles",
        ParentMenu: null,
        SubMenus: [],
        Event: 1011,
        Position: 2,
        Id: "3e4d4bbe-6ea0-48a5-9bae-6a2ebd81a782",
        CanDelete: true,
      },
      {
        Name: "Logs",
        ParentMenu: null,
        SubMenus: [],
        Event: 1730,
        Position: 3,
        Id: "5a2b13c4-2c05-4347-8e6d-3258cebdb42d",
        CanDelete: true,
      },
      {
        Name: "Settings",
        ParentMenu: null,
        SubMenus: [],
        Event: 1620,
        Position: 4,
        Id: "a03188e3-509c-41de-81d1-408729921a8f",
        CanDelete: true,
      },
    ],
    Event: null,
    Position: 0,
    Id: "e9cd458a-d707-49f1-8a57-631a85c5f79f",
    CanDelete: true,
  },
  {
    Name: "Acme",
    ParentMenu: null,
    SubMenus: null,
    Event: 2131,
    Position: 1,
    Id: "588be36f-67ba-4f84-a662-551d8de11030",
    CanDelete: true,
  },
  {
    Name: "User Roles",
    ParentMenu: null,
    SubMenus: null,
    Event: 1011,
    Position: 2,
    Id: "26608edc-5a56-4d3b-b2f1-dc3b1361663b",
    CanDelete: true,
  },
  {
    Name: "Web Sites",
    ParentMenu: null,
    SubMenus: null,
    Event: 2005,
    Position: 2,
    Id: "d722d8a6-299d-4ec3-9aad-fa2228b9b478",
    CanDelete: true,
  },
  {
    Name: "Lotto",
    ParentMenu: null,
    SubMenus: null,
    Event: 2001,
    Position: 3,
    Id: "4973ef23-36b1-4381-bf6a-3f8acb2e6a76",
    CanDelete: true,
  },
  {
    Name: "Guid",
    ParentMenu: null,
    SubMenus: null,
    Event: 2100,
    Position: 4,
    Id: "4214f108-f33a-4ad2-b683-25cdcaf6f312",
    CanDelete: true,
  },
  {
    Name: "Lotto Numbers",
    ParentMenu: null,
    SubMenus: null,
    Event: 2170,
    Position: 5,
    Id: "9f88d9b9-121b-44fc-b266-ee9edab48cae",
    CanDelete: true,
  },
] as MenuItem[];

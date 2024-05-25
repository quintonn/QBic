import {
  Button,
  Container,
  ContentLayout,
  Header,
  SpaceBetween,
  Table,
  TableProps,
  TextFilter,
} from "@cloudscape-design/components";
import { useMenus } from "../../Hooks/menuHook";
import { useEffect } from "react";

export const ViewComponent = () => {
  const columnDefinitions: TableProps.ColumnDefinition<unknown>[] = [
    {
      id: "name",
      header: "Name",
      cell: ({ id, name }) => name,
      isRowHeader: true,
      sortingField: "name",
    },
    {
      id: "id",
      header: "Id",
      cell: ({ id, name }) => id,
      isRowHeader: true,
      sortingField: "id",
    },
    {
      id: "actions",
      header: "Actions",
      minWidth: 60,
      cell: ({ id, name }) => id,
    },
  ];

  const { currentMenu } = useMenus();

  useEffect(() => {
    if (currentMenu) {
      console.log("sdfdsfsdfdsfdsfs");
    }
  }, [currentMenu]);

  return (
    <Table
      items={[]}
      columnDefinitions={columnDefinitions}
      variant="full-page"
      header={
        <Header
          variant="awsui-h1-sticky"
          actions={
            <SpaceBetween direction="horizontal" size="xs">
              {/* <Button>Back</Button> How will we handle this?? */}
              <Button variant="primary">Add</Button>
            </SpaceBetween>
          }
        >
          {currentMenu?.Description}
        </Header>
      }
      filter={<TextFilter filteringText="filter text" />}
    ></Table>
  );
};

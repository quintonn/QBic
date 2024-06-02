import {
  Button,
  CollectionPreferencesProps,
  Header,
  SpaceBetween,
  Table,
  TableProps,
  TextFilter,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import { useMenu } from "../../ContextProviders/MenuProvider/MenuProvider";
import { TablePreferences } from "./TablePreferences.component";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { ViewActionColumn } from "./ViewActionColumn";
import { ViewColumnCell } from "./ViewColumn";

// create common interface between ColumnDefintion and ContentDisplayItem
interface HasId {
  id: string;
}

// Function to create a default column
const createDefaultColumn = ({ id }: HasId) => ({
  id,
  visible: true,
});

const getDefaultPreference = (
  columns: TableProps.ColumnDefinition<unknown>[]
): CollectionPreferencesProps.Preferences => {
  return {
    stickyColumns: { first: 1, last: 1 },
    contentDisplay: columns.map((c) => createDefaultColumn(c as HasId)),
    pageSize: 20,
  };
};

// Function to check if a column exists in the provided definitions
const isColumnInDefinitions = (column: HasId, definitions: HasId[]) =>
  definitions.some((definition) => definition.id === column.id);

export const ViewComponent = () => {
  const { appName } = useMainApp();
  const [loading, setLoading] = useState(true);
  const [columnDefinitions, setColumnDefinitions] = useState<
    TableProps.ColumnDefinition<unknown>[]
  >([]);
  const [tableItems, setTableItems] = useState<any[]>([]);

  const { currentMenu } = useMenu();

  const [preferences, setPreferences] =
    useState<CollectionPreferencesProps.Preferences>(null);
  const [preferenceKey, setPreferenceKey] = useState("");

  const updatePreference = (value: CollectionPreferencesProps.Preferences) => {
    localStorage.setItem(preferenceKey, JSON.stringify(value));
    setPreferences(value);
  };

  const loadData = () => {
    const columnsToShow = currentMenu?.Columns?.filter(
      (c) => c.ColumnType == 0
    );
    const actionColumnsToShow = currentMenu?.Columns?.filter(
      (c) => c.ColumnType == 2 || c.ColumnType == 3
    );

    if (!columnsToShow) {
      return;
    }

    const viewColumns = columnsToShow.map(
      (c) =>
        ({
          id: c.ColumnName,
          header: c.ColumnLabel,
          cell: (row) => <ViewColumnCell rowData={row} column={c} />,
          maxWidth: 250,
          minWidth: 200,
        } as TableProps.ColumnDefinition<unknown>)
    );

    const cols = [
      ...viewColumns,
      {
        id: "actions",
        header: "Actions",
        cell: (rowData) => (
          <ViewActionColumn rowData={rowData} columns={actionColumnsToShow} />
        ),
      },
    ];

    const _preferenceKey = appName + "_" + currentMenu.Id + "_preference_cache";

    setPreferenceKey(_preferenceKey);
    const savedPreferencesString = localStorage.getItem(_preferenceKey);
    let updatedPreferences = getDefaultPreference(cols);
    if (savedPreferencesString) {
      try {
        updatedPreferences = JSON.parse(savedPreferencesString);
      } catch (err) {
        console.log(
          "error parsing saved preferences for menu item: ",
          currentMenu
        );
        console.log(err);
      }
    }

    // sync saved preferences with any updates made since they were saved
    if (updatedPreferences && updatedPreferences.contentDisplay) {
      // Start with columns from preferences to keep the user order and filter out deprecated columns
      const existingColumns = updatedPreferences.contentDisplay.filter(
        (column) => isColumnInDefinitions(column, cols as HasId[])
      );

      const columnDefinitionIds = cols
        .filter(({ id }) => id)
        .map(({ id }) => ({ id: id as string }));

      let preferencesContentDisplay = [...updatedPreferences.contentDisplay];
      // Add new columns from definitions
      const newColumns = columnDefinitionIds
        .filter(
          (column) => !isColumnInDefinitions(column, preferencesContentDisplay)
        )
        .map(createDefaultColumn);

      const contentDisplay = [...existingColumns, ...newColumns];

      updatedPreferences = { ...updatedPreferences, contentDisplay };
    }
    setPreferences(updatedPreferences);

    setColumnDefinitions(cols);
    setTableItems(currentMenu.ViewData);
    setLoading(false);
  };

  useEffect(() => {
    setLoading(true);

    if (currentMenu) {
      console.log("current menu is set");
      console.log(currentMenu);
      loadData();
    } else {
      console.log("current menu is not set");
    }
  }, [currentMenu]);

  return (
    <Table
      items={tableItems}
      loading={loading}
      // resizableColumns={true}
      loadingText="Loading data"
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
      filter={<TextFilter filteringText="Filter items" />}
      columnDisplay={preferences?.contentDisplay}
      contentDensity={preferences?.contentDensity}
      stripedRows={preferences?.stripedRows}
      wrapLines={preferences?.wrapLines}
      stickyColumns={preferences?.stickyColumns}
      preferences={
        <TablePreferences
          preferences={preferences}
          setPreferences={updatePreference}
          columnDisplayPreferenceOptions={
            columnDefinitions.map(({ id, header, isRowHeader }) => ({
              id,
              label: header,
              alwaysVisible: isRowHeader,
            })) as CollectionPreferencesProps.ContentDisplayPreference["options"]
          }
        />
      }
    ></Table>
  );
};

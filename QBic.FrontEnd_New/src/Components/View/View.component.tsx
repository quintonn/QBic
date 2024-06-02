import {
  Box,
  Button,
  CollectionPreferencesProps,
  FormField,
  Header,
  Pagination,
  SpaceBetween,
  Table,
  TableProps,
  TextFilter,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import {
  MenuDetail,
  MenuItem,
  useMenu,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { TablePreferences } from "./TablePreferences.component";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { ViewActionColumn } from "./ViewActionColumn";
import { ViewColumnCell } from "./ViewColumn";
import { useApi } from "../../Hooks/apiHook";
import { useDebounce } from "../../Hooks/useDebounce";

// create common interface between ColumnDefintion and ContentDisplayItem
interface HasId {
  id: string;
}

interface ViewMenu {
  Label: string;
  EventNumber: number;
  ParametersToPass: string;
  IncludeDataInView: boolean;
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

interface ViewSettings {
  totalLines: number;
  currentPage: number;
  linesPerPage: number;
  pageCount: number;
  filter?: string;
  parameters?: any;
  eventParameters?: any;
}

export const ViewComponent = () => {
  const { appName } = useMainApp();
  const [loading, setLoading] = useState(false);
  const [columnDefinitions, setColumnDefinitions] = useState<
    TableProps.ColumnDefinition<unknown>[]
  >([]);
  const [tableItems, setTableItems] = useState<any[]>([]);

  const [viewSettings, setViewSettings] = useState<ViewSettings>({
    totalLines: -1,
    currentPage: 1,
    pageCount: 1,
    linesPerPage: -1,
  });
  const [mustReload, setMustReload] = useState(false);

  const [filterText, setFilterText] = useState("");

  const { currentMenu } = useMenu();
  const api = useApi();

  const [viewMenu, setViewMenu] = useState<ViewMenu[]>([]);

  const [preferences, setPreferences] =
    useState<CollectionPreferencesProps.Preferences>(null);
  const [preferenceKey, setPreferenceKey] = useState("");

  const updatePreference = (value: CollectionPreferencesProps.Preferences) => {
    localStorage.setItem(preferenceKey, JSON.stringify(value));
    setPreferences(value);
  };

  const retrieveData = async () => {
    setLoading(true);
    try {
      const data = {
        Data: {
          viewSettings: {
            currentPage: viewSettings.currentPage,
            linesPerPage: viewSettings.linesPerPage,
            totalLines: -1, //viewSettings.totalLines,
          },
          filter: viewSettings.filter,
          parameters: viewSettings.parameters,
          eventParameters: viewSettings.eventParameters,
        },
      };
      const viewData = await api.makeApiCall<MenuDetail>(
        "updateViewData/" + currentMenu.Id,
        "POST",
        data
      );

      if (viewData) {
        setTableItems(viewData.ViewData);
        updateCounts(viewData.TotalLines);
      }
    } catch (err) {
      console.log("error loading data");
      console.log(err);
    } finally {
      setLoading(false);
    }
  };

  const loadConfig = async () => {
    const columnsToShow = currentMenu?.Columns?.filter(
      (c) => c.ColumnType == 0 || c.ColumnType == 1 // 0-string, 1-boolean
    );
    const actionColumnsToShow = currentMenu?.Columns?.filter(
      (c) => c.ColumnType == 2 || c.ColumnType == 3 // 2-button, 3-link
    );

    const hiddenColumns = currentMenu?.Columns?.filter(
      (c) => c.ColumnType == 4 // 4-hiden
    ); // not sure if we need to do anything with this

    // 5-date, 6-checkbox // maybe later can format and do other stuff

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
  };

  const loadViewMenu = async () => {
    const data = { data: currentMenu.DataForGettingMenu };
    const viewMenu = await api.makeApiCall<ViewMenu[]>(
      "getViewMenu/" + currentMenu.Id,
      "POST",
      data
    );

    setViewMenu(viewMenu);
  };

  useEffect(() => {
    if (
      mustReload == true &&
      viewSettings != null &&
      viewSettings.totalLines > -1
    ) {
      setMustReload(false);
      retrieveData();
    }
  }, [mustReload]);

  const updateCounts = (totalLines: number) => {
    let pageCount = Math.floor(totalLines / preferences.pageSize);
    let currentPage = viewSettings.currentPage;
    if (totalLines % preferences.pageSize > 0) {
      pageCount++;
    }

    if (currentPage > pageCount) {
      currentPage = pageCount;
    }

    setViewSettings({
      ...viewSettings,
      currentPage: currentPage,
      totalLines: totalLines,
      linesPerPage: preferences.pageSize,
      pageCount: pageCount,
    });
  };

  useEffect(() => {
    if (preferences && currentMenu) {
      // if (preferences.pageSize != viewSettings.linesPerPage) {
      //   pageCount = Math.floor(currentMenu.TotalLines / preferences.pageSize);
      //   if (currentMenu.TotalLines % preferences.pageSize > 0) {
      //     pageCount++;
      //   }
      // }

      // setViewSettings({
      //   ...viewSettings,
      //   totalLines: currentMenu.TotalLines,
      //   linesPerPage: preferences.pageSize,
      //   pageCount: pageCount,
      // });
      updateCounts(currentMenu.TotalLines);
      setMustReload(true);
    }
  }, [preferences, currentMenu]);

  useEffect(() => {
    if (currentMenu) {
      loadConfig();
      loadViewMenu();
    }
  }, [currentMenu]);

  const debouncedFilterChange = useDebounce(() => {
    setViewSettings({ ...viewSettings, filter: filterText });
    setMustReload(true); //TODO: This doesn't always trigger a reload for some reason (might only be in dev mode after saving code changes)
    console.log("filter");
  });

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
          counter={`(${viewSettings.totalLines})`}
          actions={
            <SpaceBetween direction="horizontal" size="xs">
              {/* <Button>Back</Button> How will we handle this?? */}
              {/* TODO: handle on click */}
              {viewMenu
                ? viewMenu.map((m, i) => (
                    <Button key={i} variant="normal">
                      {m.Label}
                    </Button>
                  ))
                : null}
            </SpaceBetween>
          }
        >
          {currentMenu?.Description}
        </Header>
      }
      filter={
        <TextFilter
          filteringPlaceholder="Filter items"
          onChange={({ detail }) => setFilterText(detail.filteringText)}
          onDelayedChange={(x) => {
            //console.log("on delayed change", x);
            // this is a bit too quick for now.
            debouncedFilterChange();
          }}
          filteringText={filterText}
        />
      }
      columnDisplay={preferences?.contentDisplay}
      contentDensity={preferences?.contentDensity}
      stripedRows={preferences?.stripedRows}
      wrapLines={preferences?.wrapLines}
      stickyColumns={preferences?.stickyColumns}
      pagination={
        <Pagination
          currentPageIndex={viewSettings.currentPage}
          pagesCount={viewSettings.pageCount}
          onChange={({ detail }) => {
            setViewSettings({
              ...viewSettings,
              currentPage: detail.currentPageIndex,
            });
            setMustReload(true);
          }}
        />
      }
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

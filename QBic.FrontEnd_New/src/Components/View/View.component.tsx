import {
  Box,
  Button,
  CollectionPreferencesProps,
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

  const [sortingColumn, setSortingColumn] =
    useState<TableProps<any>["sortingColumn"]>(null);

  const [sortingDescending, setSortingDescending] = useState(false);

  const onSortingChange: TableProps<any>["onSortingChange"] = ({
    detail: { isDescending, sortingColumn },
  }) => {
    setSortingDescending(Boolean(isDescending));
    setSortingColumn(sortingColumn);

    console.log("sort order changed");

    doReload(filterText, null, sortingColumn.sortingField, !isDescending);
  };

  const [viewSettings, setViewSettings] = useState<ViewSettings>({
    totalLines: -1,
    currentPage: 1,
    pageCount: 1,
    linesPerPage: -1,
  });

  const [filterText, setFilterText] = useState("");

  const { currentMenu, onMenuClick } = useMenu();
  const api = useApi();

  const [viewMenu, setViewMenu] = useState<ViewMenu[]>([]);

  const [preferences, setPreferences] =
    useState<CollectionPreferencesProps.Preferences>(null);
  const [preferenceKey, setPreferenceKey] = useState("");

  const updatePreferences = (value: CollectionPreferencesProps.Preferences) => {
    localStorage.setItem(preferenceKey, JSON.stringify(value));
    let currentPage = viewSettings.currentPage;
    if (viewSettings.linesPerPage < value.pageSize) {
      currentPage = 1;
    }
    setViewSettings({
      ...viewSettings,
      linesPerPage: value.pageSize,
      currentPage: currentPage,
    });
    setPreferences(value);
  };

  const doReload = (
    filter: string = "",
    _viewSettings: ViewSettings = null,
    sortColumn: string = "",
    sortAscending: boolean = true
  ) => {
    retrieveData(
      filter,
      _viewSettings || viewSettings,
      sortColumn,
      sortAscending
    );
  };

  const retrieveData = async (
    filter: string = "",
    _viewSettings: ViewSettings,
    sortColumn: string = "",
    sortAscending: boolean = true
  ) => {
    setLoading(true);
    try {
      const data = {
        Data: {
          viewSettings: {
            currentPage: _viewSettings.currentPage,
            linesPerPage: _viewSettings.linesPerPage,
            totalLines: -1,
          },
          filter: filter,
          sortColumn: sortColumn,
          sortAscending: sortAscending,
          parameters: currentMenu.Parameters,
          eventParameters: currentMenu.EventParameters,
        },
      };

      const viewData = await api.makeApiCall<MenuDetail>(
        "updateViewData/" + currentMenu.Id,
        "POST",
        data
      );

      if (viewData) {
        setTableItems(viewData.ViewData);
        updateCounts(viewData.TotalLines, _viewSettings.currentPage);
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
          sortingField: c.ColumnName.includes(".") ? null : c.ColumnName,
        } as TableProps.ColumnDefinition<unknown>)
    );

    const cols: TableProps.ColumnDefinition<unknown>[] = [
      ...viewColumns,
      {
        id: "actions",
        header: "Actions",
        cell: (rowData: any) => (
          <ViewActionColumn
            rowData={rowData}
            columns={actionColumnsToShow}
            menu={currentMenu}
          />
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

  const updateCounts = (totalLines: number, currentPage: number) => {
    let pageCount = Math.floor(totalLines / preferences.pageSize);
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
      // clear page settings
      setSortingDescending(false);
      setSortingColumn(null);
      setFilterText("");
      updateCounts(currentMenu.TotalLines, viewSettings.currentPage);

      doReload();
    }
  }, [preferences, currentMenu]);

  useEffect(() => {
    if (currentMenu) {
      loadConfig();
      loadViewMenu();
    }
  }, [currentMenu]);

  const debouncedFilterChange = useDebounce(() => {
    doReload(filterText, null, sortingColumn?.sortingField, !sortingDescending);
  });

  const viewMenuItemClick = (item: ViewMenu) => {
    const data = {
      data: item.ParametersToPass,
      parameters: {
        ViewId: item.EventNumber,
      },
    };

    onMenuClick(item.EventNumber, data);
  };

  const tableActions = viewMenu?.map((m, i) => (
    <Button key={i} variant="normal" onClick={() => viewMenuItemClick(m)}>
      {m.Label}
    </Button>
  ));

  const doRefresh = () => {
    doReload(filterText, null, sortingColumn?.sortingField, !sortingDescending);
  };
  tableActions.unshift(
    <Button
      key="refresh"
      variant="icon"
      iconName="refresh"
      onClick={doRefresh}
    ></Button>
  );

  return (
    <Table
      items={tableItems}
      loading={loading}
      // resizableColumns={true}
      loadingText="Loading data"
      columnDefinitions={columnDefinitions}
      variant="full-page"
      sortingDisabled={currentMenu?.AllowSorting === false}
      sortingColumn={sortingColumn}
      sortingDescending={sortingDescending}
      onSortingChange={onSortingChange}
      empty={
        <Box margin={{ vertical: "xs" }} textAlign="center" color="inherit">
          <SpaceBetween size="m">
            {filterText.length > 0 ? (
              <>
                <b>No matches found </b>
                <Button
                  onClick={() => {
                    setFilterText("");
                    doReload(
                      "",
                      null,
                      sortingColumn.sortingField,
                      !sortingDescending
                    );
                  }}
                >
                  Clear filter
                </Button>
              </>
            ) : (
              <b>No resources</b>
            )}
          </SpaceBetween>
        </Box>
      }
      header={
        <Header
          variant="awsui-h1-sticky"
          counter={`(${viewSettings.totalLines})`}
          actions={
            <SpaceBetween direction="horizontal" size="xs">
              {/* <Button>Back</Button> How will we handle this?? */}
              {/* TODO: handle on click */}
              {tableActions}
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
            debouncedFilterChange();
          }}
          filteringText={filterText}
          countText={
            tableItems.length === 1 ? "1 match" : tableItems.length + " matches"
          }
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
            const newViewSettings = {
              ...viewSettings,
              currentPage: detail.currentPageIndex,
            };

            setViewSettings(newViewSettings);
            doReload(
              filterText,
              newViewSettings,
              sortingColumn?.sortingField || "",
              !sortingDescending
            );
          }}
        />
      }
      preferences={
        <TablePreferences
          preferences={preferences}
          setPreferences={updatePreferences}
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

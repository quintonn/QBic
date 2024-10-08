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
  ViewColumn,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { TablePreferences } from "./TablePreferences.component";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { ViewActionColumn } from "./ViewActionColumn";
import { ViewColumnCell } from "./ViewColumn";
import { useApi } from "../../Hooks/apiHook";
import { useDebounce } from "../../Hooks/useDebounce";
import { useActions } from "../../ContextProviders/ActionProvider/ActionProvider";
import { useViewEvents } from "../../Hooks/viewEventsHook";

import orderBy from "lodash/orderBy";

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
    stickyColumns: { first: 0, last: 0 },
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
  pageCount: number;
  parameters?: any;
  eventParameters?: any;
}

interface TableComponentProps {
  menuItem: MenuDetail;
  isEmbedded?: boolean;
  defaultData?: any;
  handleOnActionColumnClick?: (
    column: ViewColumn,
    rowInfo: any
  ) => Promise<void>;
}

const test = {
  Name: "Department 1",
  Id: "03956867-301d-4758-8a49-89b69508752a",
  CanDelete: true,
};

export const TableComponent = ({
  menuItem,
  isEmbedded = false,
  defaultData = null,
  handleOnActionColumnClick = () => {
    return Promise.resolve();
  },
}: TableComponentProps) => {
  const { appName, setSelectedTableRow, displayStack, selectedRow } =
    useMainApp();

  const { handleViewEvent } = useViewEvents();
  const { onMenuClick } = useActions();
  const api = useApi();

  const [loading, setLoading] = useState(false);
  const [columnDefinitions, setColumnDefinitions] = useState<
    TableProps.ColumnDefinition<unknown>[]
  >([]);
  const [tableItems, setTableItems] = useState<any[]>([]);

  const [selectedItems, setSelectedItems] = useState<any[]>(
    selectedRow?.rowData ? [selectedRow?.rowData] : []
  );
  //const [selectedRow, setSelectedRow] = useState<any>({});

  const [sortingColumn, setSortingColumn] =
    useState<TableProps<any>["sortingColumn"]>(null);

  const [sortingDescending, setSortingDescending] = useState(false);

  const onSortingChange: TableProps<any>["onSortingChange"] = ({
    detail: { isDescending, sortingColumn },
  }) => {
    setSortingDescending(Boolean(isDescending));
    setSortingColumn(sortingColumn);

    if (isEmbedded == false) {
      doReload(filterText, null, sortingColumn.sortingField, !isDescending);
    } else {
      const sortedItems = orderBy(
        [...tableItems],
        sortingColumn.sortingField,
        isDescending ? "desc" : "asc"
      );
      setTableItems(sortedItems);
    }
  };

  const [viewSettings, setViewSettings] = useState<ViewSettings>({
    totalLines: -1,
    currentPage: 1,
    pageCount: 1,
  });

  const [filterText, setFilterText] = useState("");
  const [viewMenu, setViewMenu] = useState<ViewMenu[]>([]);

  const [preferences, setPreferences] =
    useState<CollectionPreferencesProps.Preferences>(null);

  const preferenceKey = appName + "_" + menuItem?.Id + "_preference_cache";
  const filterKey = appName + "_" + menuItem?.Id + "_filter_cache";

  const columnPreferenceKey =
    appName + "_" + menuItem?.Id + "_column_preference_cache";

  const updateFilterValue = (value: string): void => {
    localStorage.setItem(filterKey, value);
    setFilterText(value);

    if (value == null || value.length == 0) {
      localStorage.removeItem(filterKey);
    }
  };

  const updateColumnPreferences = (
    value: TableProps.ColumnWidthsChangeDetail
  ) => {
    const savedColumnPreferenceString =
      localStorage.getItem(columnPreferenceKey);

    let newColumnPreferences: any = {};
    if (savedColumnPreferenceString) {
      newColumnPreferences = JSON.parse(savedColumnPreferenceString);
    }

    for (let i = 0; i < columnDefinitions.length; i++) {
      const colDef = columnDefinitions[i];
      newColumnPreferences[colDef.id] = value.widths[i];
    }

    localStorage.setItem(
      columnPreferenceKey,
      JSON.stringify(newColumnPreferences)
    );
  };

  const updatePreferences = (value: CollectionPreferencesProps.Preferences) => {
    localStorage.setItem(preferenceKey, JSON.stringify(value));
    let currentPage = viewSettings.currentPage;
    if (value.pageSize > preferences.pageSize) {
      currentPage = 1;
    }

    setViewSettings({
      ...viewSettings,
      currentPage: currentPage,
    });
    setPreferences(value);
  };

  const doReload = async (
    filter: string = "",
    _viewSettings: ViewSettings = null,
    sortColumn: string = "",
    sortAscending: boolean = true
  ): Promise<void> => {
    await retrieveData(
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
      let data = {
        Data: {
          viewSettings: {
            currentPage: _viewSettings.currentPage,
            linesPerPage: preferences.pageSize,
            totalLines: -1,
          },
          filter: filter,
          sortColumn: sortColumn,
          sortAscending: sortAscending,
          parameters: menuItem.Parameters,
          eventParameters: menuItem.EventParameters,
        },
      };

      if (isEmbedded) {
        data = {
          Data: defaultData,
        };
      }
      const viewData = await api.makeApiCall<MenuDetail>(
        "updateViewData/" + menuItem.Id,
        "POST",
        data
      );

      if (viewData && viewData.ViewData) {
        updateItems(viewData.ViewData, _viewSettings.currentPage);
      }
    } catch (err) {
      console.log("error loading data");
      console.log(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (defaultData != null) {
      updateItems(defaultData);
    }
  }, [defaultData]);

  const updateItems = (data: any[], pageNumber: number = 1) => {
    setTableItems(data);
    //updateCounts(data.length, pageNumber);
  };

  const onActionColumnClick = async (
    column: ViewColumn,
    rowData: any[]
  ): Promise<void> => {
    setLoading(true);

    try {
      await handleOnActionColumnClick(column, rowData);
      await handleViewEvent(column, rowData, column);

      if (isEmbedded) {
        doRefresh();
      }
    } finally {
      setLoading(false);
    }
  };

  const loadConfig = async () => {
    const columnsToShow = menuItem?.Columns?.filter(
      (c) => c.ColumnType == 0 || c.ColumnType == 1 // 0-string, 1-boolean
    );
    const actionColumnsToShow = menuItem?.Columns?.filter(
      (c) => c.ColumnType == 2 || c.ColumnType == 3 // 2-button, 3-link
    );

    const hiddenColumns = menuItem?.Columns?.filter(
      (c) => c.ColumnType == 4 // 4-hiden
    ); // not sure if we need to do anything with this

    // 5-date, 6-checkbox // maybe later can format and do other stuff

    if (!columnsToShow) {
      return;
    }

    const savedPreferencesString = localStorage.getItem(preferenceKey);
    const savedColumnPreferenceString =
      localStorage.getItem(columnPreferenceKey);

    let defaultColumnPreferences: any = {};
    if (savedColumnPreferenceString) {
      defaultColumnPreferences = JSON.parse(savedColumnPreferenceString);
    }

    const viewColumns = columnsToShow.map(
      (c) =>
        ({
          id: c.ColumnName,
          header: c.ColumnLabel,
          cell: (row) => <ViewColumnCell rowData={row} column={c} />,
          width: defaultColumnPreferences[c.ColumnName] || 250,
          sortingField: c.ColumnName.includes(".") ? null : c.ColumnName,
        } as TableProps.ColumnDefinition<unknown>)
    );

    const cols: TableProps.ColumnDefinition<unknown>[] = [...viewColumns];
    if (actionColumnsToShow.length > 0) {
      cols.push({
        id: "actions",
        header: "Actions",
        cell: (rowData: any) => (
          <ViewActionColumn
            rowData={rowData}
            columns={actionColumnsToShow}
            onClick={(c, r) => onActionColumnClick(c, r)}
          />
        ),
      });
    }

    let updatedPreferences = getDefaultPreference(cols);
    if (savedPreferencesString) {
      try {
        updatedPreferences = JSON.parse(savedPreferencesString);
      } catch (err) {
        console.log(
          "error parsing saved preferences for menu item: ",
          menuItem
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
    const data = { data: menuItem.DataForGettingMenu };
    const viewMenu = await api.makeApiCall<ViewMenu[]>(
      "getViewMenu/" + menuItem.Id,
      "POST",
      data
    );

    setViewMenu(viewMenu);
  };

  const updateCounts = (totalLines: number, currentPage: number) => {
    const pageSize = preferences?.pageSize || 100000;

    let pageCount = Math.floor(totalLines / pageSize);
    if (totalLines % pageSize > 0) {
      pageCount++;
    }

    if (currentPage > pageCount) {
      currentPage = pageCount;
    }

    setViewSettings({
      ...viewSettings,
      currentPage: currentPage,
      totalLines: totalLines,
      pageCount: pageCount,
    });
  };

  useEffect(() => {
    if (preferences && menuItem) {
      // clear page settings
      setSortingDescending(false);
      setSortingColumn(null);

      updateCounts(menuItem.TotalLines, viewSettings.currentPage);

      const storedFilterValue = localStorage.getItem(filterKey);
      updateFilterValue(storedFilterValue || "");

      if (!isEmbedded) {
        doReload(storedFilterValue);
      }
    }
  }, [preferences, menuItem]);

  useEffect(() => {
    if (menuItem) {
      loadConfig();
      loadViewMenu(); // this loads even it was previously shown. I think react clears state when a component is re-rendered. Might have to store my own state for this to work in the main app somehow
    }
  }, [menuItem]);

  const debouncedFilterChange = () =>
    useDebounce(
      () => {
        doReload(
          filterText,
          null,
          sortingColumn?.sortingField,
          !sortingDescending
        );
      },
      "refresh",
      500
    );

  const viewMenuItemClick = async (item: ViewMenu) => {
    const data = {
      // data: item.ParametersToPass, // i am dissabling this because for adding in detail section, the parameters value should be what was set in parameters
      // parameters: {
      //   ViewId: item.EventNumber,
      // },
      parameters: item.ParametersToPass,
    };

    await handleOnActionColumnClick(null, null);
    await onMenuClick(item.EventNumber, data);
  };

  const tableActions = viewMenu?.map((m, i) => (
    <Button key={i} variant="normal" onClick={() => viewMenuItemClick(m)}>
      {m.Label}
    </Button>
  ));

  const doRefresh = () => {
    doReload(filterText, null, sortingColumn?.sortingField, !sortingDescending);
  };
  if (!isEmbedded) {
    tableActions.unshift(
      <Button
        key="refresh"
        variant="icon"
        iconName="refresh"
        onClick={doRefresh}
      ></Button>
    );
  }

  const onSelectionChange = (items: any[]) => {
    setSelectedItems(items);
    const selectedRow = items?.length === 1 ? items[0] : null;
    setSelectedTableRow({ menuItem: menuItem, rowData: selectedRow });
  };

  // TODO: E.g. - on Departments to view the expenses.
  //              will have to get/set via mainApp

  // TODO: I wanted to add the details section as something that can be passed from the back-end.
  //       It might have to be a child component or something as the details panel can have multiple tabs.
  //       Maybe a field that returns a list of MenuNumbers of ViewDetail classes.
  //       But maybe there's a way then to use something other than MenuNumbers so that we can prevent users from adding the wrong number by accident.
  //       Maybe ask for the types only?
  //       --> have a method with an object where users can call config.AddDetailView<T>(); // where T is the detail view type. I think this is the best.
  //           or just initialize the class like inputView

  return (
    <Table
      items={tableItems}
      loading={loading}
      resizableColumns={true}
      onColumnWidthsChange={(e) => updateColumnPreferences(e.detail)}
      loadingText="Loading data"
      columnDefinitions={columnDefinitions}
      variant={isEmbedded ? "embedded" : "full-page"}
      sortingDisabled={menuItem?.AllowSorting === false}
      sortingColumn={sortingColumn}
      sortingDescending={sortingDescending}
      onSortingChange={onSortingChange}
      onSelectionChange={({ detail }) => {
        onSelectionChange(detail.selectedItems);
      }}
      selectedItems={selectedItems}
      selectionType={isEmbedded || !menuItem.DetailSectionId ? null : "single"}
      empty={
        <Box margin={{ vertical: "xs" }} textAlign="center" color="inherit">
          <SpaceBetween size="m">
            {filterText.length > 0 ? (
              <>
                <b>No matches found </b>
                <Button
                  onClick={() => {
                    updateFilterValue("");
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
          counter={`(${
            isEmbedded ? tableItems.length : viewSettings.totalLines
          })`}
          actions={
            <SpaceBetween direction="horizontal" size="xs">
              {/* <Button>Back</Button> How will we handle this?? */}
              {/* TODO: handle on click */}
              {tableActions}
            </SpaceBetween>
          }
        >
          {menuItem?.Title}
        </Header>
      }
      filter={
        isEmbedded ? null : (
          <TextFilter
            filteringPlaceholder="Filter items"
            onChange={({ detail }) => updateFilterValue(detail.filteringText)}
            onDelayedChange={(x) => {
              if (
                x.detail.filteringText == null ||
                x.detail.filteringText == ""
              ) {
                doReload(
                  "",
                  null,
                  sortingColumn?.sortingField,
                  !sortingDescending
                );
              } else {
                debouncedFilterChange();
              }
            }}
            filteringText={filterText}
            countText={
              tableItems.length === 1
                ? "1 match"
                : tableItems.length + " matches"
            }
          />
        )
      }
      columnDisplay={preferences?.contentDisplay}
      contentDensity={preferences?.contentDensity}
      stripedRows={preferences?.stripedRows}
      wrapLines={preferences?.wrapLines}
      stickyColumns={preferences?.stickyColumns}
      pagination={
        isEmbedded ? null : (
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
        )
      }
      preferences={
        isEmbedded ? null : (
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
        )
      }
    ></Table>
  );
};

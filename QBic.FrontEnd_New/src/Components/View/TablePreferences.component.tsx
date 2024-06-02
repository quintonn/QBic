import {
  CollectionPreferences,
  CollectionPreferencesProps,
} from "@cloudscape-design/components";

export function TablePreferences({
  preferences,
  setPreferences,
  columnDisplayPreferenceOptions,
}: {
  preferences: CollectionPreferencesProps.Preferences;
  setPreferences: (preferences: CollectionPreferencesProps.Preferences) => void;
  columnDisplayPreferenceOptions: CollectionPreferencesProps.ContentDisplayPreference["options"];
}) {
  return (
    <CollectionPreferences
      title="Preferences"
      confirmLabel="Confirm"
      cancelLabel="Cancel"
      preferences={preferences}
      onConfirm={({ detail }) => setPreferences(detail)}
      pageSizePreference={{
        title: "Page size",
        options: [
          //{ value: 2, label: "2 items" },
          { value: 10, label: "10 items" },
          { value: 20, label: "20 items" },
          { value: 50, label: "50 items" },
          { value: 100, label: "100 items" },
        ],
      }}
      wrapLinesPreference={{
        label: "Wrap lines",
        description: "Select to see all the text and wrap the lines",
      }}
      stripedRowsPreference={{
        label: "Striped rows",
        description: "Select to add alternating shaded rows",
      }}
      contentDensityPreference={{
        label: "Compact mode",
        description: "Select to display content in a denser, more compact mode",
      }}
      contentDisplayPreference={{
        title: "Column preferences",
        description: "Customize the columns visibility and order.",
        options: columnDisplayPreferenceOptions,
      }}
      stickyColumnsPreference={{
        firstColumns: {
          title: "Stick first column(s)",
          description:
            "Keep the first column(s) visible while horizontally scrolling the table content.",
          options: [
            { label: "None", value: 0 },
            { label: "First column", value: 1 },
            { label: "First two columns", value: 2 },
          ],
        },
        lastColumns: {
          title: "Stick last column",
          description:
            "Keep the last column visible while horizontally scrolling the table content.",
          options: [
            { label: "None", value: 0 },
            { label: "Last column", value: 1 },
          ],
        },
      }}
    />
  );
}

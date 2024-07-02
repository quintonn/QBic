import { Box, Button, SpaceBetween } from "@cloudscape-design/components";
import {
  ColumnType,
  ViewColumn,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";
import { useViewEvents } from "../../Hooks/viewEventsHook";

export interface ViewActionColumnProps {
  rowData: any[];
  columns: ViewColumn[];
  onClick: (column: ViewColumn, rowData: any[]) => Promise<void>;
}

const ColButton = ({
  rowData,
  column,
  onClick,
}: {
  rowData: any;
  column: ViewColumn;
  onClick: (column: ViewColumn, rowData: any[]) => Promise<void>;
}) => {
  const visible = showColumn({ rowData, column });

  const { handleViewEvent } = useViewEvents();

  let label = column.ColumnLabel;
  if (
    column.ColumnType == ColumnType.Button ||
    column.ColumnType == ColumnType.Link
  ) {
    label = column.LinkLabel;
  }

  const buttonClick = () => {
    onClick(column, rowData);
  };

  return (
    <div
      style={{
        opacity: visible ? 1 : 0,
        display: "flex",
        height: "100%", // to make the link items align vertical center
      }}
    >
      <Button
        variant={
          column.ColumnType == ColumnType.Link ? "inline-link" : "normal"
        }
        onClick={buttonClick}
        wrapText={false}
        disabled={visible ? false : true} // this is so the cursor is normal when it's invisible
      >
        {label}
      </Button>
    </div>
  );
};

export const ViewActionColumn = ({
  rowData,
  columns,
  onClick,
}: ViewActionColumnProps) => {
  if (!columns || columns.length == 0) {
    return "";
  }

  return (
    <Box float="right">
      <SpaceBetween direction="horizontal" size="xl">
        {columns
          // .filter((c) => showColumn({ rowData, column: c }))
          .map((c, i) => (
            <ColButton key={i} rowData={rowData} column={c} onClick={onClick} />
          ))}
        {/* <Button
        href="#"
        variant="link"
        onClick={() => {
          console.log(rowData);
        }}
      >
        Edit
      </Button>
      <Button>X</Button> */}
      </SpaceBetween>
    </Box>
  );
};

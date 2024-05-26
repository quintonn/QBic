import { Button, SpaceBetween } from "@cloudscape-design/components";
import {
  ColumnType,
  ViewColumn,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";

export interface ViewActionColumnProps {
  rowData: any[];
  columns: ViewColumn[];
}

const ColButton = ({
  rowData,
  column,
}: {
  rowData: any;
  column: ViewColumn;
}) => {
  //console.log(column);
  if (!showColumn({ rowData, column })) {
    return <Button>X</Button>;
  }
  let label = column.ColumnLabel;
  if (
    column.ColumnType == ColumnType.Button ||
    column.ColumnType == ColumnType.Link
  ) {
    label = column.LinkLabel;
  }
  return <Button>{label}</Button>;
};

export const ViewActionColumn = ({
  rowData,
  columns,
}: ViewActionColumnProps) => {
  if (!columns || columns.length == 0) {
    return "";
  }

  // todo: show/hide columns based on settings
  return (
    <SpaceBetween direction="horizontal" size="xs">
      {columns
        // .filter((c) => showColumn({ rowData, column: c }))
        .map((c, i) => (
          <ColButton key={i} rowData={rowData} column={c} />
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
  );
};

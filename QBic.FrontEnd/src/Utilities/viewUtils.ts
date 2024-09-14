import { ViewColumn } from "../ContextProviders/MenuProvider/MenuProvider";

const parseColumnName = (columnName: string, data: any) => {
  let value = "";
  if (columnName != null && columnName.length > 0) {
    value = data;
    var colName = columnName;
    while (colName.indexOf(".") > -1) {
      var index = colName.indexOf(".");
      var partName = colName.substring(0, index);
      if (value == null) {
        break;
      }
      value = value[partName];

      colName = colName.substring(index + 1);
    }
    if (value != null) {
      value = value[colName];
    }
  }
  return value;
};

const isConditionMet = (condition: any, actualValue: any) => {
  var colName = condition.ColumnName;
  var comparison = condition.Comparison;
  var colVal = condition.ColumnValue;

  var compareResult = true;
  switch (comparison) {
    case 0: // Equals
      return actualValue == colVal;
    case 1: // Not Equals
      return actualValue != colVal;
    case 2: // Contains
      return actualValue.toLowerCase().indexOf(colVal.toLowerCase()) > -1;
    case 3: // IsNotNull
      return actualValue != null && actualValue.length > 0;
    case 4: // IsNull
      return actualValue == null || actualValue.length == 0;
    case 5: // Greater Than
      actualValue = parseInt(actualValue);
      return actualValue != null && actualValue > colVal;
    case 6: // Greater than or equal
      actualValue = parseInt(actualValue);
      return actualValue != null && actualValue >= colVal;
    case 7: // Less than
      actualValue = parseInt(actualValue);
      return actualValue != null && actualValue < colVal;
    case 8: // Less than or equal to
      actualValue = parseInt(actualValue);
      return actualValue != null && actualValue <= colVal;
    default:
      //todo: show an error somewhere
      console.error("Unknown condition comparison type: " + comparison);
      break;
  }
  return compareResult;
};

interface ShowColumnProps {
  rowData: any;
  column: ViewColumn;
}
export const showColumn = ({ rowData, column }: ShowColumnProps) => {
  if (column.ColumnSetting != null) {
    if (column.ColumnSetting.ColumnSettingType == 0) {
      /// Show/Hide column
      var show = column.ColumnSetting.Display == 0;
      var compareResult = true;

      for (var p = 0; p < column.ColumnSetting.Conditions.length; p++) {
        var condition = column.ColumnSetting.Conditions[p];
        var colName = condition.ColumnName;

        var actualValue = parseColumnName(colName, rowData) + "";

        compareResult = compareResult && isConditionMet(condition, actualValue);
      }

      if (
        (compareResult == false && show == true) ||
        (compareResult == true && show == false)
      ) {
        return false;
      }
    }
  }
  return true;
};

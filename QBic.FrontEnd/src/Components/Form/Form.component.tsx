import {
  Button,
  Checkbox,
  Container,
  DatePicker,
  FileUpload,
  Form,
  FormField,
  Header,
  Input,
  Multiselect,
  Select,
  SpaceBetween,
  Tabs,
  Textarea,
} from "@cloudscape-design/components";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useEffect, useRef, useState } from "react";

import {
  InputButton,
  InputField,
  ListSourceItem,
  MenuDetail,
  VisibilityConditions,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { useAppDispatch } from "../../App/hooks";
import { addMessage } from "../../App/flashbarSlice";
import { useApi } from "../../Hooks/apiHook";

import { OptionDefinition } from "@cloudscape-design/components/internal/components/option/interfaces";
import { useActions } from "../../ContextProviders/ActionProvider/ActionProvider";

import moment from "moment";
import { useDebounce } from "../../Hooks/useDebounce";
import { TableComponent } from "../View/Table.component";

interface FormCacheData {
  fieldName: string;
  cacheValue: Record<string, any>;
  rowId: number; // for deletion
}

interface FormComponentProps {
  menuItem: MenuDetail;
}

export const FormComponent = ({ menuItem }: FormComponentProps) => {
  const mainApp = useMainApp();
  const [loading, setLoading] = useState(false);
  const dispatch = useAppDispatch();

  const [errorMessage, setErrorMessage] = useState("");

  const [errors, setErrors] = useState<Record<string, string | null>>({});
  const [values, setValues] = useState<Record<string, any>>({});
  const [fieldVisibility, setFieldVisibility] = useState<
    Record<string, boolean>
  >({});
  const [fieldSources, setFieldSources] = useState<
    Record<string, ListSourceItem[]>
  >({});
  const api = useApi();
  const { handleAction } = useActions();

  const valuesRef = useRef<Record<string, any>>({});

  const [formCache, setFormCache] = useState<FormCacheData>(null);

  valuesRef.current = values;

  const getInputValue = async (
    field: InputField,
    onChangeValue = null
  ): Promise<any> => {
    let value = null;
    if (onChangeValue != null) {
      value = onChangeValue;
    } else if (valuesRef.current[field.InputName] != null) {
      value = valuesRef.current[field.InputName];
    }

    if (field.InputType == 3) {
      // combo box
      const selectedItem = value as OptionDefinition;
      if (selectedItem) {
        value = selectedItem?.value;
      }
    } else if (field.InputType == 5) {
      // list selection
      const selectedItems = value as OptionDefinition[];
      value = selectedItems?.filter((s) => s != null).map((s) => s.value);
    } else if (field.InputType == 6) {
      // date
      if (value) {
        try {
          value = new Date(value).toISOString().split("T")[0]; // example date = 2024-07-21T00:00:00.000Z
        } catch (err) {
          console.log(`error parsing date ${value}: ${err}`);
        }

        //TODO: make sure dates are working - test adding/editing/prop changed, etc

        // This code is so it works in Odyssey I think
        // const date = new Date(value);
        // value =
        //   String(date.getDate()).padStart(2, "0") +
        //   "-" +
        //   String(date.getMonth() + 1).padStart(2, "0") +
        //   "-" +
        //   String(date.getFullYear()).padStart(2, "0");
        // TODO: revert this
      }
    } else if (field.InputType == 8) {
      // input view
      value = valuesRef.current[field.InputName];
      return value;
    } else if (field.InputType == 9) {
      // file

      if (value && value.length > 0) {
        value = await readFile(value[0]);
        return value;
      }
    }
    return value;
  };

  const readFile = async (file: any) => {
    return new Promise(function (resolve, reject) {
      const reader = new FileReader();

      reader.onload = function (e) {
        const arrayBuffer = e.target.result as ArrayBuffer;
        const byteArray = new Uint8Array(arrayBuffer);

        // Convert Uint8Array to binary string
        let binaryString = "";
        for (let i = 0; i < byteArray.length; i++) {
          binaryString += String.fromCharCode(byteArray[i]);
        }

        const base64String = window.btoa(binaryString);

        const filename = file.name;
        const parts = filename.split(".");
        const extension = parts.length > 1 ? parts[parts.length - 1] : "";
        const nameWithoutExtension = parts[0];

        const result = {
          Data: base64String,
          FileName: nameWithoutExtension,
          MimeType: file.type,
          FileExtension: extension,
          Size: file.size,
        };

        resolve(result);
      };

      reader.onerror = function (err) {
        reject(err);
      };

      reader.readAsArrayBuffer(file);
    }).catch(function (err) {
      console.error(err);
      dispatch(
        addMessage({
          type: "error",
          content: `Error reading file, see logs for details (${err})`,
        })
      );
    });
  };

  const getInputValues = async (includeAll: boolean = false) => {
    const result: any = {};

    for (let i = 0; i < menuItem.InputFields.length; i++) {
      const field = menuItem.InputFields[i];

      if (includeAll != true) {
        const isVisible = fieldVisibility[field.InputName];
        if (!isVisible) {
          continue;
        }
      }
      result[field.InputName] = await getInputValue(field);
    }

    return result;
  };

  const onButtonClick = async (button: InputButton) => {
    setLoading(true);

    // TODO: conditional mandatory setting
    // TODO: test that all inputs and features are working

    try {
      if (button.ValidateInput) {
        //todo : validate all input fields -> currentMenu.fields with values
        for (let i = 0; i < menuItem.InputFields.length; i++) {
          const field = menuItem.InputFields[i];
          const value = await getInputValue(field);
          const validationResult = validateField(field, value);
          if (validationResult.length > 0) {
            return;
          }
        }
      }

      const params = await getInputValues();

      const data: any = {
        Data: params || "",
        ActionId: button.ActionNumber,
      };

      // call api
      const resp = await api.makeApiCall<MenuDetail[]>(
        "processEvent/" + menuItem.Id,
        "POST",
        data
      );

      //await formEvents.handleEvents(resp);
      for (let i = 0; i < resp?.length; i++) {
        handleAction(resp[i]);
      }
    } catch (error) {
      console.log(error);
      let message = "Unexpected error: " + error;
      dispatch(
        addMessage({
          type: "error",
          content: message,
        })
      );
    } finally {
      setLoading(false);
    }
  };

  const validateField = (
    field: InputField,
    value: any,
    forcedCheck: boolean = false
  ): string => {
    if (
      field.Mandatory === true &&
      (fieldVisibility[field.InputName] !== false || forcedCheck === true)
    ) {
      if (value === null || value === "") {
        return field.InputLabel + " is required";
      }
      if (field.InputType == 3) {
      } else if (field.InputType == 5) {
        if (value == null || value.length == 0) {
          return field.InputLabel + " is required";
        }
      } else if (field.InputType == 9) {
        if (value == null || value.length == 0) {
          return field.InputLabel + " is required";
        }
      }
    }

    return "";
  };

  const buildInputs = () => {
    const fields = menuItem.InputFields;

    const defaultVisibility = fields.reduce((prev, f) => {
      prev[f.InputName] = true;
      return prev;
    }, {});

    const defaultFieldSources = fields
      .filter((f) => f.InputType == 5 || f.InputType == 3)
      .reduce((prev, f) => {
        prev[f.InputName] = f.InputType == 5 ? f.ListSource : f.ListItems;
        return prev;
      }, {});

    setFieldSources(defaultFieldSources);

    setFieldVisibility(defaultVisibility);

    fields.forEach((f) => {
      let defaultValue = f.DefaultValue;

      switch (f.InputType) {
        case 3: {
          // combo box
          if (f.DefaultValue) {
            defaultValue = {
              label: f.ListItems.filter((l) => l.Key == f.DefaultValue)[0]
                ?.Value,
              value: f.DefaultValue,
            };
          } else {
            defaultValue = null;
          }
          break;
        }
        case 4: {
          // boolean
          defaultValue = f.DefaultValue === true ? true : false;
          break;
        }
        case 5: {
          // multi select
          let listDefaultValue = f.DefaultValue || "";
          if (listDefaultValue.constructor.name != "Array") {
            listDefaultValue = listDefaultValue.split(",");
          }
          const tmpDefaultValues = listDefaultValue as string[];

          defaultValue = tmpDefaultValues
            .map((d) => {
              const label = f.ListSource.filter((l) => l.Key == d)[0]?.Value;
              return { label: label, value: d };
            })
            .filter((x) => x.label && x.label != "");
          break;
        }
        case 6: {
          // date input
          defaultValue = f.DefaultValue || "";
          if (defaultValue) {
            // convert from mainApp.dateFormat
            try {
              //defaultValue = parse("23-06-2024", "dd-MM-YYYY", new Date());
              defaultValue = moment(defaultValue, "DD-MMM-YYYY").format(
                "YYYY-MM-DD"
              );
            } catch (err) {
              console.log("error parsing date: " + defaultValue);
              console.log(err);
            }
          }

          break;
        }
        case 8: {
          // input view, do nothing extra
          break;
        }
        case 9: {
          // file input
          defaultValue = [];
          break;
        }
        default:
          break;
      }
      onChange(f, defaultValue, true); // to update the visbility
    });
  };

  const [dummy, setDummy] = useState("");

  useEffect(() => {
    if (menuItem) {
      buildInputs();
      setDummy("reload-done"); // using this because we need various state values set before calling the onChange method (i.e. getting/setting the value state variable)
    }
  }, [menuItem]);

  useEffect(() => {
    if (dummy == "reload-done") {
      const fields = menuItem.InputFields;

      const valuesToUse = formCache?.cacheValue ?? values;
      fields.forEach((f) => {
        const fieldValue = valuesToUse[f.InputName];
        onChange(f, fieldValue); // raise on property changed now to update visibility conditions etc
      });

      setDummy("");
    }
  }, [dummy]);

  useEffect(() => {
    if (mainApp.inputViewUpdateData != null && formCache != null) {
      const currentValue = values[formCache.fieldName] || [];
      const inputViewUpdateData = mainApp.inputViewUpdateData;

      let newValue = [];

      if (inputViewUpdateData == null) {
        // this was a cancellation
        return;
      }

      if (inputViewUpdateData === -1) {
        // delete item
        newValue = currentValue;
        newValue.splice(formCache.rowId, 1);
      } else if (formCache.rowId == -1) {
        // this is for a new item
        let rowId = -1;
        for (let j = 0; j < currentValue.length; j++) {
          rowId = Math.max(rowId, currentValue[j].rowId);
        }
        rowId++;

        inputViewUpdateData.rowId = rowId;
        newValue = [...currentValue, inputViewUpdateData];
      } else {
        // this is when an item is modified
        const rowIndex = formCache.rowId;
        newValue = currentValue;
        inputViewUpdateData.rowId = rowIndex;
        newValue[rowIndex] = inputViewUpdateData;
      }

      // update all rowIds
      let index = 0;
      for (let j = 0; j < newValue.length; j++) {
        newValue[j].rowId = index;
        index++;
      }

      setValues((prevValues) => ({
        ...prevValues,
        [formCache.fieldName]: newValue,
      }));
    }
  }, [mainApp.inputViewUpdateData]);

  const conditionIsMet = (condition: VisibilityConditions, value: any) => {
    if (condition.Comparison == 0) {
      // Equals
      return value === condition.ColumnValue;
    } else if (condition.Comparison == 1) {
      // Not-Equals
      return value != condition.ColumnValue;
    } else if (condition.Comparison == 2) {
      // Contains
      return value.indexOf(condition.ColumnValue) > -1;
    } else if (condition.Comparison == 3) {
      // IsNotNull
      return value != null && (value + "").length > 0;
    } else if (condition.Comparison == 4) {
      // IsNull
      return value == null || (value + "").length == 0;
    } else {
      console.log("Error", "Unknown comparison: " + condition.Comparison);
      return false;
    }
  };

  const raisePropertyChangedEvent = async (field, fieldValue) => {
    setLoading(true);
    try {
      const data = {
        Data: {
          PropertyName: field.InputName,
          PropertyValue: fieldValue,
          EventId: menuItem.Id,
        },
      };
      const resp = await api.makeApiCall<MenuDetail[]>(
        "propertyChanged/" + menuItem.Id,
        "POST",
        data
      );

      if (resp && resp.length > 0) {
        for (let i = 0; i < resp.length; i++) {
          let item = resp[i];
          switch (item.ActionType) {
            case 10: {
              // update combo box
              setFieldSources((prevValues) => ({
                ...prevValues,
                [item.InputName]: item.ListItems,
              }));

              const fld = menuItem.InputFields.filter(
                (f) => f.InputName == item.InputName
              )[0];
              const currentValue = await getInputValue(fld);

              if (currentValue) {
                if (fld.InputType == 3) {
                  const validValues = item.ListItems.map((l) => l.Key);
                  if (!validValues.includes(currentValue)) {
                    setValues((prevValues) => ({
                      ...prevValues,
                      [item.InputName]: null,
                    }));
                  }
                } else if (fld.InputType == 5) {
                  const validValues = item.ListItems.map((l) => l.Key);

                  let newCurrentValue = values[item.InputName] as any[];
                  const updatedValue = newCurrentValue.filter((n) =>
                    validValues.includes(n.value)
                  );

                  if (!validValues.includes(currentValue)) {
                    setValues((prevValues) => ({
                      ...prevValues,
                      [item.InputName]: updatedValue,
                    }));
                  }
                }
              }
              break;
            }
            case 17: {
              // UpdateInputVisibility
              setFieldVisibility((prevValues) => ({
                ...prevValues,
                [item.InputName]: item.InputIsVisible,
              }));
              break;
            }
            default: {
              console.log("unknown action type: " + item.ActionType);
              dispatch(
                addMessage({
                  type: "error",
                  content: "Unknown action type: " + item.ActionType,
                })
              );
              break;
            }
          }
        }
      }
    } finally {
      setLoading(false);
    }
  };
  const debouncedPropertyChanged = (propName, timeout, field, ...x) =>
    useDebounce(raisePropertyChangedEvent, propName, timeout, ...[field, ...x]);

  const updateFieldVisibilities = (
    fieldChanged: InputField,
    valueChanged: any
  ) => {
    const otherFields = menuItem.InputFields.filter(
      (f) =>
        f.VisibilityConditions.filter(
          (v) => v.ColumnName == fieldChanged.InputName
        ).length > 0
    );

    for (let i = 0; i < otherFields.length; i++) {
      const field = otherFields[i];

      const matchedConditions = field.VisibilityConditions.filter((condition) =>
        conditionIsMet(condition, valueChanged?.toString())
      );

      if (matchedConditions.length > 0) {
        const otherValue = values[field.InputName];
        updateFieldErrors(field, otherValue, true);
      }

      setFieldVisibility((prevValues) => ({
        ...prevValues,
        [field.InputName]: matchedConditions.length > 0,
      }));
    }
  };

  useEffect(() => {
    if (errors) {
      const keys = Object.keys(errors);

      let thereAreErrors = false;
      for (let i = 0; i < keys.length; i++) {
        const error = errors[keys[i]];
        const visibility = fieldVisibility[keys[i]];

        if (error && visibility === true) {
          thereAreErrors = true;
        }
      }

      if (thereAreErrors === true) {
        setErrorMessage(
          "There are invalid inputs. Make sure to check all tabs."
        );
      } else {
        setErrorMessage("");
      }
    }
  }, [errors]);

  const updateFieldErrors = async (
    field: InputField,
    value: any,
    forcedValidation: boolean = false // because this might be checked while changing visibility
  ) => {
    const actualValue = await getInputValue(field, value);
    const fieldError = validateField(field, actualValue, forcedValidation);

    setErrors((prevErrors) => ({
      ...prevErrors,
      [field.InputName]: fieldError,
    }));
  };

  const onChange = async (
    field: InputField,
    value: any,
    isScreenSetup: boolean = false
  ) => {
    setValues((prevValues) => ({ ...prevValues, [field.InputName]: value }));

    await updateFieldErrors(field, value);

    if (isScreenSetup === true) {
      return; // don't handle property changed events or check visibility conditions during screen setup
    }

    const fieldValue = await getInputValue(field, value);
    updateFieldVisibilities(field, fieldValue);

    if (field.RaisePropertyChangedEvent === true) {
      let timeoutValue = 0;
      if (
        field.InputType == 0 ||
        field.InputType == 1 ||
        field.InputType == 10
      ) {
        timeoutValue = 500;
      }
      debouncedPropertyChanged(
        field.InputName,
        timeoutValue,
        field,
        fieldValue
      );
    }
  };

  const onInputViewColumnActionClick = async (
    field: InputField,
    rowData: any
  ): Promise<void> => {
    const currentInputs = valuesRef.current;

    mainApp.setInputViewUpdateData(null); // clear any values here

    const cacheItem: FormCacheData = {
      fieldName: field.InputName,
      cacheValue: currentInputs,
      rowId: rowData?.rowId ?? -1,
    };

    setFormCache(cacheItem);

    return Promise.resolve();
  };

  const getInputField = (field: InputField, setAutoFocus: boolean) => {
    switch (field.InputType) {
      case 0: // text
      case 1: // password
        if (field.MultiLineText == true) {
          return (
            <Textarea
              placeholder={field.InputLabel}
              autoFocus={setAutoFocus}
              value={values?.[field.InputName]}
              onChange={({ detail: { value } }) => onChange(field, value)}
            ></Textarea>
          );
        }
        return (
          <Input
            placeholder={field.InputLabel}
            autoFocus={setAutoFocus}
            type={field.InputType == 1 ? "password" : "text"}
            value={values?.[field.InputName]}
            onChange={({ detail: { value } }) => onChange(field, value)}
          />
        );
      case 2: // hidden input
        return null;
      case 3: // combo box
        return (
          <Select
            autoFocus={setAutoFocus}
            selectedOption={values?.[field.InputName]}
            onChange={({ detail }) => {
              onChange(field, detail.selectedOption);
            }}
            options={fieldSources[field.InputName]?.map((x) => ({
              label: x.Value,
              value: x.Key,
            }))}
          ></Select>
        );
      case 4: // boolean
        return (
          <Checkbox
            onChange={({ detail }) => onChange(field, detail.checked)}
            checked={values?.[field.InputName]}
          >
            {field.InputLabel}
          </Checkbox>
        );
      case 5: // list selection input
        return (
          <Multiselect
            tokenLimit={10}
            autoFocus={setAutoFocus}
            selectedOptions={values?.[field.InputName]}
            onChange={({ detail }) => {
              onChange(field, detail.selectedOptions);
            }}
            options={fieldSources[field.InputName]?.map((x) => ({
              label: x.Value,
              value: x.Key,
            }))}
          ></Multiselect>
        );
      case 6: // date input
        return (
          <DatePicker
            //locale="en-CA" // en-CA uses the date format YYYY-MM-DD, which is what we use (i think)
            value={values?.[field.InputName]}
            autoFocus={setAutoFocus}
            onChange={({ detail: { value } }) => onChange(field, value)}
            placeholder={field.InputLabel}
          />
        );
      case 8: // Input View
        return (
          <Container>
            <TableComponent
              menuItem={field.ViewForInput}
              isEmbedded={true}
              defaultData={values?.[field.InputName]}
              handleOnActionColumnClick={(c, data) =>
                onInputViewColumnActionClick(field, data)
              }
            ></TableComponent>
          </Container>
        );
      case 9: // file input
        return (
          <FileUpload
            onChange={({ detail: { value } }) => onChange(field, value)}
            value={values?.[field.InputName]}
            i18nStrings={{
              uploadButtonText: (e) => (e ? "Choose files" : "Choose file"),
              dropzoneText: (e) =>
                e ? "Drop files to upload" : "Drop file to upload",
              removeFileAriaLabel: (e) => `Remove file ${e + 1}`,
              limitShowFewer: "Show fewer files",
              limitShowMore: "Show more files",
              errorIconAriaLabel: "Error",
            }}
            showFileLastModified={false}
            showFileSize
            showFileThumbnail
          />
        );
      case 10: // number
        return (
          <Input
            autoFocus={setAutoFocus}
            type="number"
            placeholder={field.InputLabel}
            value={values?.[field.InputName]}
            onChange={({ detail: { value } }) => onChange(field, value)}
          />
        );
      case 11: // label
        return <div>{field.DefaultValue}</div>;
      default:
        console.error("unhandled input type: " + field.InputType);
        console.log(field);

        dispatch(
          addMessage({
            type: "error",
            content:
              "Unknown input type: " +
              field.InputType +
              " for field " +
              field.InputLabel,
          })
        );
        return null;
    }
  };

  const getTabContent = (tabName: string) => {
    const fields = menuItem.InputFields.filter(
      (x) => x.TabName == tabName || (tabName == null && x.TabName == "")
    );

    const formFields = fields
      .filter((f) => f.InputType != 2 /* exclude hidden fields */)
      .map((f, i) => (
        <div key={f.InputName}>
          {fieldVisibility[f.InputName] != true ? null : (
            <FormField
              key={f.InputName}
              label={f.InputLabel}
              errorText={errors?.[f.InputName]}
            >
              {getInputField(f, i == 0)}
            </FormField>
          )}
        </div>
      ));

    return <SpaceBetween size="l">{formFields}</SpaceBetween>;
  };

  const getTabs = () => {
    if (menuItem == null) {
      return null;
    }

    // Get unique tab names, ignore hidden inputs
    const uniqueTabs = new Set<string>();
    menuItem.InputFields.forEach((field) => {
      if (field.InputType != 2) {
        uniqueTabs.add(field.TabName || "");
      }
    });

    const tabNames = Array.from(uniqueTabs?.values()).filter(
      (x) => x != null && x.length > 0
    );

    return (
      <>
        {tabNames.length > 0 ? (
          <Tabs
            tabs={tabNames.map((t) => ({
              label: t,
              id: t,
              content: <SpaceBetween size="l">{getTabContent(t)}</SpaceBetween>,
            }))}
          />
        ) : null}

        <SpaceBetween size="l">{getTabContent(null)}</SpaceBetween>
      </>
    );
  };

  const onFormSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    // button events handle the submit case
  };

  return (
    <form onSubmit={onFormSubmit}>
      <Form
        variant="full-page"
        header={<Header variant="awsui-h1-sticky">{menuItem?.Title}</Header>}
        // errorText="Some error"
        errorText={errorMessage}
        actions={
          <SpaceBetween direction="horizontal" size="xs" alignItems="end">
            {menuItem?.InputButtons?.map((b) => (
              <Button
                key={b.Label}
                variant="primary"
                loading={loading}
                onClick={() => onButtonClick(b)}
              >
                {b.Label}
              </Button>
            ))}
          </SpaceBetween>
        }
      >
        {getTabs()}
      </Form>
    </form>
  );
};

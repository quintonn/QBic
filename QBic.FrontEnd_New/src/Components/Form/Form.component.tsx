import {
  Button,
  Checkbox,
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
import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
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

export const FormComponent = () => {
  const mainApp = useMainApp();
  const [loading, setLoading] = useState(false);
  const location = useLocation();
  const [currentMenu, setCurrentMenu] = useState<MenuDetail>();
  const dispatch = useAppDispatch();

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

  const getInputValue = async (
    field: InputField,
    onChangeValue = null
  ): Promise<any> => {
    let value = null;
    if (onChangeValue != null) {
      value = onChangeValue;
    } else if (values[field.InputName] != null) {
      value = values[field.InputName];
    }
    //let value = onChangeValue || values[field.InputName] || null;

    if (field.InputType == 3) {
      // combo box
      const selectedItem = value as OptionDefinition;
      value = selectedItem.value;
    } else if (field.InputType == 5) {
      // list selection
      const selectedItems = value as OptionDefinition[];
      value = selectedItems.map((s) => s.value);
    } else if (field.InputType == 6) {
      // date
      if (value) {
        value = new Date(value).toISOString().split("T")[0];
      }
    } else if (field.InputType == 9) {
      // file
      if (value && value.length > 0) {
        value = await readFile(value[0]);
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

  const getInputValues = async () => {
    const result: any = {};

    for (let i = 0; i < currentMenu.InputFields.length; i++) {
      const field = currentMenu.InputFields[i];

      const isVisible = fieldVisibility[field.InputName];
      if (!isVisible) {
        continue;
      }
      result[field.InputName] = await getInputValue(field);
    }

    return result;
  };

  const onButtonClick = async (button: InputButton) => {
    setLoading(true);

    try {
      if (button.ValidateInput) {
        //todo : validate all input fields -> currentMenu.fields with values
        for (let i = 0; i < currentMenu.InputFields.length; i++) {
          const field = currentMenu.InputFields[i];
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
        "processEvent/" + currentMenu.Id,
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

  const validateField = (field: InputField, value: any): string => {
    if (field.Mandatory === true && fieldVisibility[field.InputName] == true) {
      if (value === null || value === "") {
        return field.InputLabel + " is required";
      }
    }
    return "";
  };

  const buildInputs = () => {
    const fields = currentMenu.InputFields;

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
          defaultValue = {
            label: f.ListItems.filter((l) => l.Key == f.DefaultValue)[0]?.Value,
            value: f.DefaultValue,
          };
          break;
        }
        case 4: {
          // boolean
          defaultValue = f.DefaultValue === true ? true : false;
          break;
        }
        case 5: {
          // multi select
          const tmpDefaultValues = f.DefaultValue || ([] as string[]);
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
        case 9: {
          // file input
          defaultValue = [];
        }
        default:
          break;
      }

      onChange(f, defaultValue); // to update the visbility
    });
  };

  useEffect(() => {
    if (location && location.pathname) {
      const menuItem = mainApp.getCacheValue(location.pathname);
      setCurrentMenu(menuItem);
    }
  }, [location]);

  useEffect(() => {
    if (currentMenu) {
      buildInputs();
    }
  }, [currentMenu]);

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
          EventId: currentMenu.Id,
        },
      };
      const resp = await api.makeApiCall<MenuDetail[]>(
        "propertyChanged/" + currentMenu.Id,
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

              const fld = currentMenu.InputFields.filter(
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
    const otherFields = currentMenu.InputFields.filter(
      (f) =>
        f.VisibilityConditions.filter(
          (v) => v.ColumnName == fieldChanged.InputName
        ).length > 0
    );

    for (let i = 0; i < otherFields.length; i++) {
      const field = otherFields[i];

      const matchedConditions = field.VisibilityConditions.filter(
        (condition) => {
          return conditionIsMet(condition, valueChanged?.toString());
        }
      );

      setFieldVisibility((prevValues) => ({
        ...prevValues,
        [field.InputName]: matchedConditions.length > 0,
      }));
    }
  };

  const onChange = async (field: InputField, value: any) => {
    setValues((prevValues) => ({ ...prevValues, [field.InputName]: value }));

    const fieldError = validateField(field, value);
    setErrors((prevErrors) => ({
      ...prevErrors,
      [field.InputName]: fieldError,
    }));

    updateFieldVisibilities(field, value);

    if (field.RaisePropertyChangedEvent === true) {
      const fieldValue = await getInputValue(field, value);
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
            value={values?.[field.InputName]}
            autoFocus={setAutoFocus}
            onChange={({ detail: { value } }) => onChange(field, value)}
            placeholder={field.InputLabel}
          />
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
    const fields = currentMenu.InputFields.filter((x) => x.TabName == tabName);
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
    if (currentMenu == null) {
      return null;
    }

    // Get unique tab names, ignore hidden inputs
    const uniqueTabs = new Set<string>();
    currentMenu.InputFields.forEach((field) => {
      if (field.InputType != 2) {
        uniqueTabs.add(field.TabName || "");
      }
    });

    const tabNames = Array.from(uniqueTabs?.values()).filter(
      (x) => x != null && x.length > 0
    );

    return (
      <>
        {tabNames.length > 1 ? (
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
        header={<Header variant="awsui-h1-sticky">{currentMenu?.Title}</Header>}
        // errorText="Some error"
        actions={
          <SpaceBetween direction="horizontal" size="xs" alignItems="start">
            {currentMenu?.InputButtons?.map((b) => (
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
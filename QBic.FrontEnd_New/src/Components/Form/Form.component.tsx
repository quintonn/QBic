import {
  Button,
  Checkbox,
  Form,
  FormField,
  Header,
  Input,
  Multiselect,
  Select,
  SpaceBetween,
  Tabs,
} from "@cloudscape-design/components";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import {
  InputButton,
  InputField,
  MenuDetail,
  VisibilityConditions,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { useAppDispatch } from "../../App/hooks";
import { addMessage } from "../../App/flashbarSlice";
import { useApi } from "../../Hooks/apiHook";

import { OptionDefinition } from "@cloudscape-design/components/internal/components/option/interfaces";
import { useActions } from "../../ContextProviders/ActionProvider/ActionProvider";

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
  const api = useApi();
  const { handleAction } = useActions();

  const getInputValues = () => {
    const result: any = {};

    for (let i = 0; i < currentMenu.InputFields.length; i++) {
      const field = currentMenu.InputFields[i];

      const isVisible = fieldVisibility[field.InputName];
      if (!isVisible) {
        continue;
      }
      let value = values[field.InputName];

      if (field.InputType == 3) {
        const selectedItem = value as OptionDefinition;
        value = selectedItem.value;
      } else if (field.InputType == 5) {
        const selectedItems = value as OptionDefinition[];
        value = selectedItems.map((s) => s.value);
      }

      result[field.InputName] = value;
    }

    return result;
  };

  const onButtonClick = async (button: InputButton) => {
    setLoading(true);

    try {
      if (button.ValidateInput) {
        //todo : validate all input fields -> currentMenu.fields with values
      }
      //TODO: handle button click
      //      if validateInput, validateAllFields

      // get inputs, then process event

      // TODO: handle visibility conditions (e.g. adding/editing menu items)
      //       Might be worth creating different objects to represent each input or something

      const params = getInputValues();

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
      for (let i = 0; i < resp.length; i++) {
        handleAction(resp[i]);
      }
    } catch (error) {
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
    if (field.Mandatory === true) {
      if (value == null || value == "") {
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

  const onChange = (field: InputField, value: any) => {
    setValues((prevValues) => ({ ...prevValues, [field.InputName]: value }));

    const fieldError = validateField(field, value);
    setErrors((prevErrors) => ({
      ...prevErrors,
      [field.InputName]: fieldError,
    }));

    updateFieldVisibilities(field, value);

    if (field.RaisePropertyChangedEvent === true) {
      //todo - old code only triggered on field exit I THINK!!
    }
  };

  const getInputField = (field: InputField) => {
    switch (field.InputType) {
      case 0: // text
      case 1: // password
        return (
          <Input
            type={field.InputType == 1 ? "password" : "text"}
            value={values?.[field.InputName]}
            onChange={({ detail: { value } }) => onChange(field, value)}
          />
        );
      case 2: // hidden input
        return null; // shouldn't have this
      case 3: // combo box
        return (
          <Select
            selectedOption={values?.[field.InputName]}
            onChange={({ detail }) => {
              onChange(field, detail.selectedOption);
            }}
            options={field.ListItems?.map((x) => ({
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
            selectedOptions={values?.[field.InputName]}
            onChange={({ detail }) => {
              onChange(field, detail.selectedOptions);
            }}
            options={field.ListSource?.map((x) => ({
              label: x.Value,
              value: x.Key,
            }))}
          ></Multiselect>
        );
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
    const formFields = fields.map((f) => (
      <div key={f.InputName}>
        {fieldVisibility[f.InputName] != true ? null : (
          <FormField
            key={f.InputName}
            label={f.InputLabel}
            errorText={errors?.[f.InputName]}
          >
            {getInputField(f)}
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

    const tabNames = Array.from(uniqueTabs?.values());
    if (tabNames.length > 1) {
      return (
        <Tabs
          tabs={tabNames.map((t) => ({
            label: t,
            id: t,
            content: getTabContent(t),
          }))}
        />
      );
    }

    return <SpaceBetween size="l">{getTabContent(null)}</SpaceBetween>;
    //TODO: I think there are scenarios where some fields should display for all tabs, i.e. not on the tabs but in a main component
    // I think if tab name is '' (empty string)
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

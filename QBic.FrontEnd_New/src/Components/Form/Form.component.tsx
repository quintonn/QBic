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
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { useAppDispatch } from "../../App/hooks";
import { addMessage } from "../../App/flashbarSlice";
import { useApi } from "../../Hooks/apiHook";

import { OptionDefinition } from "@cloudscape-design/components/internal/components/option/interfaces";
import { useActions } from "../../Hooks/actionHook";

export const FormComponent = () => {
  const mainApp = useMainApp();
  const [loading, setLoading] = useState(false);
  const location = useLocation();
  const [currentMenu, setCurrentMenu] = useState<MenuDetail>();
  const dispatch = useAppDispatch();

  const [errors, setErrors] = useState<Record<string, string | null>>({});
  const [values, setValues] = useState<Record<string, any>>({});
  const api = useApi();
  const { handleAction } = useActions();

  const getInputValues = () => {
    const result: any = {};

    for (let i = 0; i < currentMenu.InputFields.length; i++) {
      const field = currentMenu.InputFields[i];

      let value = values[field.InputName];

      if (field.InputType == 3) {
        console.log(field);
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
    console.log("on button click");
    console.log(button);
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
      console.log(resp);
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
    console.log("build inputs");
    const fields = currentMenu.InputFields;
    console.log(fields);
    const defaultValues = fields.reduce((prev, f) => {
      if (f.InputType == 3) {
        // combo box
        prev[f.InputName] = {
          label: f.ListItems.filter((l) => l.Key == f.DefaultValue)[0]?.Value,
          value: f.DefaultValue,
        };
      } else if (f.InputType == 4) {
        // boolean
        prev[f.InputName] = f.DefaultValue === true ? true : false;
      } else if (f.InputType == 5) {
        // multi select
        const defaultValues = f.DefaultValue || ([] as string[]);
        const tmp = defaultValues
          .map((d) => {
            const label = f.ListSource.filter((l) => l.Key == d)[0]?.Value;
            return { label: label, value: d };
          })
          .filter((x) => x.label && x.label != "");
        prev[f.InputName] = tmp;
      } else {
        prev[f.InputName] = f.DefaultValue || "";
      }
      return prev;
    }, {});
    console.log(defaultValues);
    setValues(defaultValues);
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

  const onChange = (field: InputField, value: any) => {
    setValues((prevValues) => ({ ...prevValues, [field.InputName]: value }));

    const fieldError = validateField(field, value);
    setErrors((prevErrors) => ({
      ...prevErrors,
      [field.InputName]: fieldError,
    }));

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
      <FormField
        key={f.InputName}
        label={f.InputLabel}
        errorText={errors?.[f.InputName]}
      >
        {getInputField(f)}
      </FormField>
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

import {
  Button,
  Form,
  FormField,
  Header,
  Input,
  Multiselect,
  SpaceBetween,
  Spinner,
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

export const FormComponent = () => {
  const mainApp = useMainApp();
  const [loading, setLoading] = useState(false);
  const location = useLocation();
  const [currentMenu, setCurrentMenu] = useState<MenuDetail>();
  const dispatch = useAppDispatch();

  const [errors, setErrors] = useState<Record<string, string | null>>({});
  const [values, setValues] = useState<Record<string, any>>({});

  const onButtonClick = (button: InputButton) => {
    console.log("on button click");
    console.log(button);
    setLoading(true);

    if (button.ValidateInput) {
      //todo : validate all input fields -> currentMenu.fields with values
    }
    //TODO: handle button click
    //      if validateInput, validateAllFields

    // get inputs, then process event

    const params = values;
    var data: any = {
      Data: params || "",
      ActionId: button.ActionNumber,
    };
    data = JSON.stringify(data);
    console.log(data);

    setTimeout(() => {
      setLoading(false);
    }, 3000);
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
    const defaultValues = fields.reduce((prev, f) => {
      if (f.InputType == 5) {
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

import {
  Alert,
  Box,
  Button,
  Container,
  ContentLayout,
  Form,
  FormField,
  Grid,
  Header,
  Input,
  Modal,
  SpaceBetween,
} from "@cloudscape-design/components";
import { useState } from "react";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useApi } from "../../Hooks/apiHook";
import { useAuth } from "../../Hooks/authHook";

interface LoginData {
  username: string;
  password: string;
}

function validateField(field: string, value: unknown): string | null {
  switch (field) {
    case "username":
      return value ? null : "Username is required";
    case "password":
      return value ? null : "Password is required";
    default:
      return null;
  }
}

function validateFormData(data: any) {
  const fields = Object.keys(data);
  const errors: Record<string, string> = fields.reduce((errors, field) => {
    const error = validateField(field, data[field]);
    if (error) {
      errors[field] = error;
    }
    return errors;
  }, {} as Record<string, string>);
  return Object.keys(errors).length > 0 ? errors : undefined;
}

export const Login = () => {
  const [values, setValues] = useState<LoginData>({
    username: "",
    password: "",
  });
  const [errors, setErrors] = useState<Record<string, string | null>>({});
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const api = useApi();
  const auth = useAuth();

  const onChange = (field: string, value: any) => {
    console.log("on change");
    setValues((prevValues) => ({ ...prevValues, [field]: value }));

    const error = validateField(field, value);
    setErrors((prevErrors) => ({ ...prevErrors, [field]: error }));
  };

  const submitForm = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const errors = validateFormData(values);

    if (errors) {
      setErrors(errors);
    } else {
      setLoading(true);

      try {
        console.log("loggging in");
        await auth.doLogin(values.username, values.password);
      } catch (err) {
        setErrorMessage(err);
      } finally {
        setLoading(false);
      }

      //   try {
      //     const response = await api.post("api/customers/", formData);
      //     navigate("..");
      //   } catch (err: any) {
      //     console.log("Error creating customer");
      //     console.log(err);
      //     if (err?.response?.data) {
      //       setApiError(err.response.data);
      //     } else {
      //       setApiError("Error creating customer: " + err);
      //     }
      //   } finally {
      //     setLoading(false);
      //   }
    }
  };

  return (
    <ContentLayout
      header={
        <SpaceBetween size="m">
          <Header
            variant="h1"
            // description="This is a generic description used in the header."
            // actions={<Button variant="primary">Button</Button>}
          ></Header>
        </SpaceBetween>
      }
    >
      <Container header={<Header variant="h2">Login</Header>}>
        <form onSubmit={submitForm}>
          <Form
            actions={
              <SpaceBetween direction="horizontal" size="xs">
                <Button variant="primary" loading={loading}>
                  Login
                </Button>
              </SpaceBetween>
            }
          >
            <SpaceBetween direction="vertical" size="l">
              <FormField label="Username" errorText={errors?.username}>
                <Input
                  onChange={({ detail: { value } }) =>
                    onChange("username", value)
                  }
                  placeholder="Enter username"
                  value={values.username}
                />
              </FormField>
              <FormField label="Password" errorText={errors?.password}>
                <Input
                  type="password"
                  onChange={({ detail: { value } }) =>
                    onChange("password", value)
                  }
                  placeholder="Enter password"
                  value={values.password}
                />
              </FormField>
              {errorMessage ? (
                <Alert
                  statusIconAriaLabel="Error"
                  type="error"
                  header={`Unable to login`}
                  key="alert-item"
                  dismissible
                  onDismiss={() => setErrorMessage("")}
                >
                  {errorMessage}
                </Alert>
              ) : null}
            </SpaceBetween>
          </Form>
        </form>
      </Container>
    </ContentLayout>
  );
};

import {
  Alert,
  Button,
  Container,
  ContentLayout,
  Form,
  FormField,
  Header,
  Input,
  Link,
  SpaceBetween,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import { useApi } from "../../Hooks/apiHook";
import { useAuth } from "../../Hooks/authHook";
import { ForgotPassword } from "./ForgotPassword.component";
import { useNavigate } from "react-router-dom";

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

  const [showForgotPassword, setShowForgotPassword] = useState(false);

  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (auth.isAuthenticated == true) {
      navigate("/");
    }
  }, [auth.isAuthenticated]);

  const onChange = (field: string, value: any) => {
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
        await auth.doLogin(values.username, values.password);
      } catch (err) {
        setErrorMessage(err);
      } finally {
        setLoading(false);
      }
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
      <>
        <ForgotPassword
          visible={showForgotPassword}
          onDismiss={() => setShowForgotPassword(false)}
        ></ForgotPassword>
        <Container header={<Header variant="h2">Login</Header>}>
          <form onSubmit={submitForm}>
            <Form
              variant="embedded"
              actions={
                <SpaceBetween
                  alignItems="center"
                  direction="horizontal"
                  size="xs"
                >
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
                <FormField>
                  <Link onFollow={() => setShowForgotPassword(true)}>
                    Forgot password?
                  </Link>
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
      </>
    </ContentLayout>
  );
};

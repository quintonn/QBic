import {
  Alert,
  Box,
  Button,
  Form,
  FormField,
  Input,
  Modal,
  SpaceBetween,
} from "@cloudscape-design/components";
import { useState } from "react";
import { useAuth } from "../../ContextProviders/AuthProvider/AuthProvider";

interface ForgotPasswordProps {
  visible: boolean;
  onDismiss: () => void;
}

interface ForgotPasswordData {
  username: string;
}

function validateField(field: string, value: unknown): string | null {
  switch (field) {
    case "username":
      return value ? null : "Username is required";
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

const initialValues: ForgotPasswordData = {
  username: "",
};

export const ForgotPassword = ({
  visible = false,
  onDismiss,
}: ForgotPasswordProps) => {
  const [values, setValues] = useState(initialValues);

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string | null>>({});

  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const auth = useAuth();

  const onChange = (field: string, value: any) => {
    console.log("on change");
    setValues((prevValues) => ({ ...prevValues, [field]: value }));

    const error = validateField(field, value);
    setErrors((prevErrors) => ({ ...prevErrors, [field]: error }));
  };

  const submitForm = async () => {
    const errors = validateFormData(values);

    if (errors) {
      setErrors(errors);
    } else {
      // todo
      setLoading(true);
      const result = await auth.resetPassword(values.username);
      if (result.includes("No user")) {
        setErrorMessage(result);
      } else {
        setSuccessMessage(result);
      }

      try {
      } catch (err) {
        console.log(err);
        setErrorMessage("Unable to reset password: " + err);
      } finally {
        setLoading(false);
      }
    }
  };

  const dismissAndClear = () => {
    setErrorMessage("");
    setSuccessMessage("");
    setValues(initialValues);
    onDismiss();
  };

  return (
    <Modal
      header="Forgot password"
      visible={visible}
      onDismiss={dismissAndClear}
      footer={
        <Box float="left">
          <SpaceBetween direction="horizontal" size="xs">
            <Button
              variant="primary"
              onClick={submitForm}
              loading={loading}
              loadingText="Resetting password"
            >
              Reset Password
            </Button>
            <Button variant="link" onClick={dismissAndClear}>
              Cancel
            </Button>
          </SpaceBetween>
        </Box>
      }
    >
      <SpaceBetween direction="vertical" size="l">
        <FormField label="Username" errorText={errors?.username}>
          <Input
            onChange={({ detail: { value } }) => onChange("username", value)}
            placeholder="Enter username"
            value={values.username}
          />
        </FormField>
        {errorMessage ? (
          <Alert
            statusIconAriaLabel="Error"
            type="error"
            header={`Error resetting password`}
            key="alert-item"
            dismissible
            onDismiss={() => setErrorMessage("")}
          >
            {errorMessage}
          </Alert>
        ) : null}
        {successMessage ? (
          <Alert
            statusIconAriaLabel="Success"
            type="success"
            header={`Success`}
            key="alert-item"
            dismissible
            onDismiss={() => setSuccessMessage("")}
          >
            {successMessage}
          </Alert>
        ) : null}
      </SpaceBetween>
    </Modal>
  );
};

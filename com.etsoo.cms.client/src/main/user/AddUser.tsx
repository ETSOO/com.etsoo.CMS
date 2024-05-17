import { ComboBox, EditPage, InputField, TextFieldEx } from "@etsoo/materialui";
import { FormControlLabel, Grid, Switch } from "@mui/material";
import React from "react";
import { useFormik } from "formik";
import { DataTypes, IdActionResult, Utils } from "@etsoo/shared";
import { useNavigate, useParams } from "react-router-dom";
import { app } from "../../app/MyApp";
import { UserUpdateDto } from "../../api/dto/user/UserUpdateDto";
import * as Yup from "yup";

function AddUser() {
  // Route
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  // Is editing
  const isEditing = id != null;
  type DataType = DataTypes.AddAndEditType<UserUpdateDto>;

  // Roles
  var roles = app.getLocalRoles();

  // Labels
  const labels = app.getLabels(
    "noChanges",
    "role",
    "enabled",
    "id",
    "deleteConfirm",
    "user",
    "password",
    "passwordTip"
  );

  // Edit data
  const [data, setData] = React.useState<DataType>({
    role: 0,
    password: "",
    enabled: true
  });

  // Form validation schema
  const validationSchema = Yup.object({
    password: Yup.string().test((value, context) => {
      if (!isEditing && (value == null || !app.isValidPassword(value))) {
        return context.createError({ message: labels.passwordTip });
      }
      return true;
    })
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Correct for types safety
      Utils.correctTypes(rq, {
        role: "number"
      });

      let result: IdActionResult | undefined;
      if (rq.id != null && isEditing) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        result = await app.userApi.update(rq);
      } else {
        result = await app.userApi.create(rq as any);
      }

      // Submit
      if (result == null) return;

      if (result.ok) {
        if (isEditing) {
          navigate("./../../all");
        } else {
          navigate("./../all");
        }
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const loadData = async () => {
    if (id == null) return;
    const data = await app.userApi.updateRead(id);
    if (data == null) return;
    setData(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? "editUser" : "addUser");

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        data.refreshTime
          ? undefined
          : () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.user),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.userApi.delete(id);
                  if (result == null) return;

                  if (result.ok) {
                    navigate("./../../all");
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
      }
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={loadData}
    >
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="id"
          required
          disabled={isEditing}
          inputProps={{ maxLength: 128 }}
          label={labels.id}
          value={formik.values.id ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <ComboBox
          options={roles}
          name="role"
          label={labels.role}
          idValue={formik.values.role}
          inputRequired
          inputOnChange={formik.handleChange}
        />
      </Grid>
      {!isEditing && (
        <Grid item xs={12} sm={6}>
          <TextFieldEx
            name="password"
            label={labels.password}
            showPassword
            autoComplete="new-password"
            variant="outlined"
            value={formik.values.password}
            onChange={formik.handleChange}
            required
            error={formik.touched.password && Boolean(formik.errors.password)}
            helperText={formik.touched.password && formik.errors.password}
          />
        </Grid>
      )}
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="enabled"
              checked={formik.values.enabled ?? true}
              onChange={formik.handleChange}
            />
          }
          label={labels.enabled}
        />
      </Grid>
    </EditPage>
  );
}

export default AddUser;

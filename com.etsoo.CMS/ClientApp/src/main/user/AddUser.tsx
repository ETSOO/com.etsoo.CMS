import { ComboBox, EditPage, InputField } from '@etsoo/materialui';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { DataTypes, Utils } from '@etsoo/shared';
import { IActionResult } from '@etsoo/appscript';
import { useNavigate, useParams } from 'react-router-dom';
import { app } from '../../app/MyApp';
import { UserUpdateDto } from '../../dto/UserUpdateDto';

function AddUser() {
  // Route
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  // Is editing
  const isEditing = id != null;
  type EditData = DataTypes.AddOrEditType<UserUpdateDto, typeof isEditing>;

  // Roles
  var roles = app.getLocalRoles();

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'role',
    'enabled',
    'id',
    'deleteConfirm',
    'user'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    id: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<EditData>({
    id,
    enabled: true,
    refreshTime: new Date()
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<EditData>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Correct for types safety
      Utils.correctTypes(rq, {
        role: 'number'
      });

      if (isEditing) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
      }

      // Submit
      const result = await app.api.put<IActionResult>(
        isEditing ? 'User/Update' : 'User/Create',
        rq
      );
      if (result == null) return;

      if (result.ok) {
        if (isEditing) {
          navigate(app.transformUrl('/home/user/all'));
        } else {
          // Reset password
          app.resetPassword(rq.id!, () => {
            navigate(app.transformUrl('/home/user/all'));
          });
        }
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const loadData = async () => {
    if (id == null) return;
    app.api.get<UserUpdateDto>('User/UpdateRead/' + id).then((data) => {
      if (data == null) return;
      setData(data);
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? 'editUser' : 'addUser');

    return () => {
      app.pageExit();
    };
  }, []);

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

                  const result = await app.api.delete<IActionResult>(
                    `User/Delete/${id}`
                  );
                  if (result == null) return;

                  if (result.ok) {
                    navigate!(app.transformUrl('/home/user/all'));
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
          value={formik.values.id ?? ''}
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

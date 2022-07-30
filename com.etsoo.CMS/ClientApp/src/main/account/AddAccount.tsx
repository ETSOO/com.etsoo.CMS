import { IActionResult, UserRole } from '@etsoo/appscript';
import { EditPage, InputField, useParamsEx } from '@etsoo/react';
import { DataTypes, Utils } from '@etsoo/shared';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import { useFormik } from 'formik';
import React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import * as Yup from 'yup';
import { app } from '../../app/MyApp';
import { Account } from '../../dto/Account';

function AddAccount() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  // Queries
  const [search] = useSearchParams();

  // Kind
  const kind = search.get('kind');
  const companyAccount = kind === 'c';

  // Permissions
  const financePermission = app.hasPermission([
    UserRole.Finance,
    UserRole.Founder
  ]);

  // Is editing
  const isEditing = id != null;
  type AccountData = DataTypes.AddOrEditType<Account, typeof isEditing>;

  // Labels
  const labels = app.getLabels(
    'displayName',
    'accountBank',
    'accountName',
    'accountNumber',
    'accountBalance',
    'externalAccount',
    'noChanges',
    'enabled',
    'requiredField',
    'save',
    'delete',
    'deleteConfirm',
    'account',
    'none'
  );

  // Edit data
  // For create, set the default value here, not in the input file value
  const [data, setData] = React.useState<AccountData>({
    accountName:
      (companyAccount ? app.serviceUser?.organizationName : undefined) ?? '',
    externalAccount: !companyAccount,
    balanceEditable: true,
    enabled: true,
    none: false
  });

  // Form validation schema
  const validationSchema = Yup.object({
    accountBank: Yup.string().required(
      labels.requiredField.format(labels.accountBank)
    ),
    accountName: Yup.string().required(
      labels.requiredField.format(labels.accountName)
    ),
    accountNumber: Yup.string().required(
      labels.requiredField.format(labels.accountNumber)
    )
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<AccountData>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

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
      const result = await app.serviceApi.put<IActionResult>(
        isEditing ? 'Account/Update' : 'Account/Create',
        rq
      );
      if (result == null) return;

      if (result.ok) {
        navigate!(
          app.transformUrl(
            `/home/account/company?kind=${values.externalAccount ? 'e' : 'c'}`
          )
        );
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = async () => {
    if (id == null) return;
    const data = await app.serviceApi.get<Account>('Account/UpdateRead/' + id);
    if (data == null) return;
    setData(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? 'editAccount' : 'addAccount');

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={() => {
        app.notifier.confirm(
          labels.deleteConfirm.format(labels.account),
          undefined,
          async (ok) => {
            const id = formik.values.id;
            if (!ok || id == null) return;

            const result = await app.serviceApi.delete<IActionResult>(
              `Account/Delete/${id}`
            );
            if (result == null) return;

            if (result.ok) {
              navigate!(
                app.transformUrl(
                  `/home/account/company?kind=${
                    formik.values.externalAccount ? 'e' : 'c'
                  }`
                )
              );
              return;
            }

            app.alertResult(result);
          }
        );
      }}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="accountName"
          inputProps={{ maxLength: 128 }}
          label={labels.accountName}
          value={formik.values.accountName}
          onChange={formik.handleChange}
          error={
            formik.touched.accountName && Boolean(formik.errors.accountName)
          }
          helperText={formik.touched.accountName && formik.errors.accountName}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="displayName"
          inputProps={{ maxLength: 128 }}
          label={labels.displayName}
          value={formik.values.displayName ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="accountBank"
          inputProps={{ maxLength: 128 }}
          label={labels.accountBank}
          value={formik.values.accountBank ?? ''}
          disabled={formik.values.none}
          onChange={formik.handleChange}
          error={
            formik.touched.accountBank && Boolean(formik.errors.accountBank)
          }
          helperText={formik.touched.accountBank && formik.errors.accountBank}
        />
      </Grid>
      {!companyAccount && (
        <Grid item xs={12} sm={6}>
          <FormControlLabel
            control={
              <Switch
                name="none"
                checked={formik.values.none ?? false}
                onChange={(event, checked) => {
                  if (checked) {
                    formik.values.accountBank = 'XXX';
                    formik.values.accountNumber = 'XXX';
                  } else {
                    formik.values.accountBank = '';
                    formik.values.accountNumber = '';
                  }

                  formik.handleChange(event);
                }}
              />
            }
            label={labels.none}
          />
        </Grid>
      )}
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="accountNumber"
          inputProps={{ maxLength: 20 }}
          disabled={formik.values.isSystem || formik.values.none}
          label={labels.accountNumber}
          value={formik.values.accountNumber ?? ''}
          onChange={formik.handleChange}
          error={
            formik.touched.accountNumber && Boolean(formik.errors.accountNumber)
          }
          helperText={
            formik.touched.accountNumber && formik.errors.accountNumber
          }
        />
      </Grid>
      {!formik.values.externalAccount &&
        financePermission &&
        formik.values.balanceEditable && (
          <Grid item xs={12} sm={6}>
            <InputField
              fullWidth
              name="accountBalance"
              type="number"
              label={labels.accountBalance}
              value={formik.values.accountBalance ?? ''}
              onChange={formik.handleChange}
            />
          </Grid>
        )}
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="externalAccount"
              checked={formik.values.externalAccount}
              onChange={formik.handleChange}
            />
          }
          disabled={kind != null}
          label={labels.externalAccount}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="enabled"
              checked={formik.values.enabled}
              onChange={formik.handleChange}
            />
          }
          label={labels.enabled}
        />
      </Grid>
    </EditPage>
  );
}

export default AddAccount;

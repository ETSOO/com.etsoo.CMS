import { EditPage } from '@etsoo/react';
import * as Yup from 'yup';
import { useFormik } from 'formik';
import React from 'react';
import { app } from '../../app/MyApp';
import { IActionResult } from '@etsoo/appscript';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import { Utils } from '@etsoo/shared';
import { UpdateSettingsRQ } from '../../RQ/UpdateSettingsRQ';
import { useNavigate } from 'react-router-dom';

function Settings() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'operationSucceeded',
    'noChanges',
    'externalAccountRequired',
    'leaderApprovalRequired',
    'writeOffRequired',
    'supportForeignCurrency'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Settings
  const initialValues = app.serviceUser ?? {};

  // Formik
  const formik = useFormik<UpdateSettingsRQ>({
    initialValues,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Changed fields
      const fields: string[] = Utils.getDataChanges(rq, initialValues);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.serviceApi.put<IActionResult>(
        'System/UpdateSettings',
        rq
      );
      if (result == null) return;

      // Sync, no necessary to await
      app.refreshToken();

      app.notifier.succeed(labels.operationSucceeded, undefined, () => {
        navigate(app.transformUrl('/home/'));
      });
    }
  });

  React.useEffect(() => {
    // Page title
    app.setPageKey('settings');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <EditPage
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
    >
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="leaderApprovalRequired"
              value={true}
              checked={formik.values.leaderApprovalRequired ?? false}
              onChange={formik.handleChange}
            />
          }
          label={labels.leaderApprovalRequired}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="writeOffRequired"
              value={true}
              checked={formik.values.writeOffRequired ?? false}
              onChange={formik.handleChange}
            />
          }
          label={labels.writeOffRequired}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="externalAccountRequired"
              value={true}
              checked={formik.values.externalAccountRequired ?? false}
              onChange={formik.handleChange}
            />
          }
          label={labels.externalAccountRequired}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="supportForeignCurrency"
              value={true}
              checked={formik.values.supportForeignCurrency ?? false}
              onChange={formik.handleChange}
            />
          }
          label={labels.supportForeignCurrency}
        />
      </Grid>
    </EditPage>
  );
}

export default Settings;

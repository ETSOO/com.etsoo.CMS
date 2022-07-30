import {
  ComboBox,
  EditPage,
  InputField,
  SwitchAnt,
  Tiplist,
  useParamsEx
} from '@etsoo/react';
import { DataTypes, DateUtils, Utils } from '@etsoo/shared';
import React from 'react';
import { app } from '../../app/MyApp';
import { AccountLine } from '../../dto/AccountLine';
import * as Yup from 'yup';
import { useFormik } from 'formik';
import {
  BusinessUtils,
  EntityStatus,
  IActionResult,
  IdLabelDto
} from '@etsoo/appscript';
import { Grid } from '@mui/material';
import { Subject } from '../../dto/Subject';
import { Account } from '../../dto/Account';
import { useNavigate, useSearchParams } from 'react-router-dom';

interface LocalStates {
  kind: string;
  isCollection: boolean;
}

function AddLine() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  // Queries
  const [search] = useSearchParams();

  // Init kind
  const initKind = search.get('kind') ?? 'e';

  // States
  const [states, updateStates] = React.useReducer(
    (currentState: LocalStates, newState: Partial<LocalStates>) => {
      return { ...currentState, ...newState };
    },
    { kind: initKind, isCollection: initKind === 'i' }
  );

  // Is editing
  const isEditing = id != null;
  type AccountLineData = DataTypes.AddOrEditType<AccountLine, typeof isEditing>;

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'requiredField',
    'save',
    'delete',
    'deleteConfirm',
    'record',
    'title',
    'subject',
    'amount',
    'happenDate',
    'repeat',
    'reference',
    'payment',
    'collection',
    'paymentAccount',
    'receivingAccount',
    'companyAccount',
    'noData'
  );

  // Edit data
  // For create, set the default value here, not in the input file value
  const [data, setData] = React.useState<AccountLineData>({
    kind: states.kind,
    happenDate: new Date()
  });

  // Single user model
  const isSingleUser =
    !app.serviceUser?.leaderApprovalRequired &&
    !app.serviceUser?.writeOffRequired;

  // Company account and external account label
  const companyAccountLabel = states.isCollection
    ? labels.receivingAccount
    : labels.paymentAccount;
  const externalAccountLabel = states.isCollection
    ? labels.paymentAccount
    : labels.receivingAccount;

  // Repeat options
  const repeatOptions = BusinessUtils.getRepeatOptions(app.labelDelegate, [
    'MONTH',
    'YEAR',
    'QUATER'
  ]);

  // Subjects
  const [subjects, setSubjects] = React.useState<Subject[]>([]);

  // Company accounts
  const [companyAccounts, setCompanyAccounts] = React.useState<Account[]>([]);

  // Form validation schema
  const validationSchema = Yup.object({
    subjectId: Yup.number().required(
      labels.requiredField.format(labels.subject)
    ),
    title: Yup.string().required(labels.requiredField.format(labels.title)),
    amount: Yup.number()
      .required(labels.requiredField.format(labels.amount))
      .min(0.01, labels.noData),
    ...(isSingleUser && {
      companyAccount: Yup.string().required(
        labels.requiredField.format(labels.companyAccount)
      )
    })
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<AccountLineData>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Set isCollection
      rq.isCollection = states.isCollection;

      if (isEditing) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
      }

      // Correct for types safety
      Utils.correctTypes(rq, {
        subjectId: 'number',
        amount: 'number',
        repeat: 'number',
        happenDate: 'date'
      });

      // Submit
      const result = await app.serviceApi.put<IActionResult>(
        isEditing ? 'AccountLine/Update' : 'AccountLine/Create',
        rq
      );
      if (result == null) return;

      if (result.ok) {
        navigate!(app.transformUrl(`/home/account/lines?kind=${states.kind}`));
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = () => {
    // subjects
    Promise.all([
      app.serviceApi.get<Subject[]>(
        'System/QuerySubjects/' + (states.kind === 'e' ? 'false' : 'true')
      ),
      app.serviceApi.get<Account[]>('Account/CompanyAccounts')
    ]).then(([subjects, accounts]) => {
      if (subjects != null) setSubjects(subjects);
      if (accounts != null) setCompanyAccounts(accounts);

      // Edit data
      if (id != null) {
        app.serviceApi
          .get<AccountLine>('AccountLine/UpdateRead/' + id)
          .then((data) => {
            if (data == null) return;
            updateStates({ kind: data.kind, isCollection: data.isCollection });
            setData(data);
          });
      }
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey(states.kind === 'e' ? 'registerExpense' : 'registerIncome');

    // Subjects
    app.serviceApi
      .get<Subject[]>(
        'System/QuerySubjects/' + (states.kind === 'e' ? 'false' : 'true')
      )
      .then((subjects) => {
        if (subjects != null) setSubjects(subjects);
      });
  }, [states.kind]);

  React.useEffect(() => {
    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        formik.values.entityStatus === 0
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.record),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.serviceApi.delete<IActionResult>(
                    `AccountLine/Delete/${id}`
                  );
                  if (result == null) return;

                  if (result.ok) {
                    navigate!(app.transformUrl(`/home/account/lines`));
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid item xs={12} sm={6}>
        <ComboBox
          name="subjectId"
          options={subjects}
          label={labels.subject}
          labelField="name"
          idValue={formik.values.subjectId}
          inputRequired
          inputOnChange={formik.handleChange}
          inputError={
            formik.touched.subjectId && Boolean(formik.errors.subjectId)
          }
          inputHelperText={formik.touched.subjectId && formik.errors.subjectId}
        />
      </Grid>
      <Grid item xs={12} sm={6} alignItems="center" display="flex">
        <SwitchAnt
          name="IsCollection"
          startLabel={labels.payment}
          endLabel={labels.collection}
          checked={states.isCollection}
          onChange={(_event, checked) =>
            updateStates({ isCollection: checked })
          }
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="title"
          multiline
          rows={2}
          inputProps={{ maxLength: 128 }}
          label={labels.title}
          value={formik.values.title ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.title && Boolean(formik.errors.title)}
          helperText={formik.touched.title && formik.errors.title}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="amount"
          type="number"
          label={labels.amount}
          value={formik.values.amount ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.amount && Boolean(formik.errors.amount)}
          helperText={formik.touched.amount && formik.errors.amount}
          inputProps={{ min: 0.01, step: 0.01, max: 99999999 }}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="happenDate"
          type="date"
          label={labels.happenDate}
          value={DateUtils.formatForInput(formik.values.happenDate)}
          onChange={formik.handleChange}
          error={formik.touched.happenDate && Boolean(formik.errors.happenDate)}
          helperText={formik.touched.happenDate && formik.errors.happenDate}
        />
      </Grid>
      {(isSingleUser ||
        (formik.values.entityStatus ?? 0) === EntityStatus.Audited) && (
        <React.Fragment>
          <Grid item xs={12} sm={6}>
            <ComboBox
              name="companyAccount"
              options={companyAccounts}
              label={companyAccountLabel}
              getOptionLabel={(option) =>
                option.accountBank + ', ' + option.accountNumber
              }
              idValue={formik.values.companyAccount}
              inputRequired
              inputOnChange={formik.handleChange}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Tiplist
              label={externalAccountLabel}
              name="externalAccount"
              idValue={formik.values.externalAccount}
              loadData={async (keyword, id) => {
                return await app.serviceApi.post<IdLabelDto[]>(
                  'Account/List',
                  {
                    id,
                    keyword
                  },
                  { defaultValue: [], showLoading: false }
                );
              }}
              inputOnChange={formik.handleChange}
              inputRequired={app.serviceUser?.externalAccountRequired}
            />
          </Grid>
        </React.Fragment>
      )}

      <Grid item xs={12} sm={6}>
        <ComboBox
          name="repeat"
          options={repeatOptions}
          label={labels.repeat}
          idValue={formik.values.repeat}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="reference"
          label={labels.reference}
          value={formik.values.reference ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}

export default AddLine;

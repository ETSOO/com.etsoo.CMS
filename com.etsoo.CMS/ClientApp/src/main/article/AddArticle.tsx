import { ComboBox, EditPage, InputField } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import { DataTypes, Utils } from '@etsoo/shared';
import { IdActionResult } from '@etsoo/appscript';
import { useNavigate } from 'react-router-dom';
import { app } from '../../app/MyApp';
import { TabSelector } from '../../components/TabSelector';
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from '@etsoo/react';
import { ArticleUpdateDto } from '../../api/dto/article/ArticleUpdateDto';
import { EOEditorElement, EOEditorEx } from '@etsoo/reacteditor';

function AddTab() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  const { tab } = useSearchParamsEx({ tab: 'number' });
  const [tabs, setTabs] = React.useState<number[]>();
  const editorRef = React.useRef<EOEditorElement>(null);

  // Is editing
  const isEditing = id != null;
  type DataType = DataTypes.AddAndEditType<ArticleUpdateDto>;

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'enabled',
    'id',
    'tab',
    'articleTitle',
    'articleSubtitle',
    'articleKeywords',
    'articleDescription',
    'articleUrl',
    'articleRelease',
    'articleLogo',
    'slideshowLogo',
    'entityStatus'
  );

  // Edit data
  const [data, setData] = React.useState<DataType>({
    tab1: 0,
    title: '',
    content: '',
    status: 0,
    release: new Date()
  });

  // Input refs
  const refFields = [
    'title',
    'subtitle',
    'url',
    'logo',
    'description',
    'keywords',
    'release'
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Request data
      const content = editorRef.current?.value;
      if (!content) {
        editorRef.current?.restoreFocus();
        return;
      }
      const rq = { ...values, content };

      ReactUtils.updateRefValues(refs, rq);

      let result: IdActionResult | undefined;
      if (rq.id != null) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        result = await app.articleApi.update(rq);
      } else {
        result = await app.articleApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        editorRef.current?.clearBackup();
        navigate(isEditing ? './../../all' : './../all');
        return;
      }

      app.alertResult(result);
    }
  });

  const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    const urlInput = refs.url.current;
    if (urlInput == null) return;
    if (!isEditing || urlInput.value === '') {
      const title = event.target.value;
      app.formatUrl(title).then((url) => {
        if (url != null) urlInput.value = url;
      });
    }
  };

  // Load data
  const loadData = async () => {
    if (id == null) {
      ReactUtils.updateRefs(refs, data);
      return;
    }

    const read = await app.articleApi.updateRead(id, {
      dateFields: ['release']
    });

    if (read == null) return;

    ReactUtils.updateRefs(refs, read);

    setData(read);

    if (read.tab1 != null) ancestorRead(read.tab1);
  };

  const ancestorRead = (tab: number) => {
    app.tabApi.ancestorRead(tab).then((data) => {
      if (data == null) return;
      data.reverse();
      setTabs(data);
    });
  };

  React.useEffect(() => {
    if (tab) {
      ancestorRead(tab);
    }
  }, [tab]);

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? 'editArticle' : 'addArticle');

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={loadData}
    >
      <Grid container item xs={12} sm={12} spacing={1}>
        <TabSelector
          name="tab1"
          label={labels.tab}
          values={tabs}
          onChange={(value) => formik.setFieldValue('tab1', value)}
          required
          error={formik.touched.tab1 && Boolean(formik.errors.tab1)}
          helperText={formik.touched.tab1 && formik.errors.tab1}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="title"
          inputProps={{ maxLength: 256 }}
          inputRef={refs.title}
          label={labels.articleTitle}
          onBlur={handleBlur}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="subtitle"
          inputProps={{ maxLength: 256 }}
          inputRef={refs.subtitle}
          label={labels.articleSubtitle}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="url"
          inputProps={{ maxLength: 128 }}
          inputRef={refs.url}
          label={labels.articleUrl}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <EOEditorEx
          ref={editorRef}
          content={formik.values.content ?? ''}
          backupKey={`article-${isEditing}`}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="logo"
          inputProps={{ maxLength: 256 }}
          inputRef={refs.logo}
          label={labels.articleLogo}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          multiline
          rows={2}
          name="description"
          inputProps={{ maxLength: 1024 }}
          inputRef={refs.description}
          label={labels.articleDescription}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="keywords"
          inputProps={{ maxLength: 256 }}
          inputRef={refs.keywords}
          label={labels.articleKeywords}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <ComboBox
          name="status"
          options={app.getStatusList()}
          label={labels.entityStatus}
          idValue={formik.values.status ?? 0}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="release"
          type="datetime-local"
          label={labels.articleRelease}
          inputRef={refs.release}
        />
      </Grid>
    </EditPage>
  );
}

export default AddTab;

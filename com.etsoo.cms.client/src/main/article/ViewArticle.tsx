import { ButtonLink, ViewPage } from '@etsoo/materialui';
import { GridDataType, useParamsEx } from '@etsoo/react';
import { DateUtils } from '@etsoo/shared';
import { Typography } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import { ArticleViewDto } from '../../api/dto/article/ArticleViewDto';
import EditIcon from '@mui/icons-material/Edit';
import CollectionsIcon from '@mui/icons-material/Collections';
import PhotoIcon from '@mui/icons-material/Photo';
import { LocalUtils } from '../../app/LocalUtils';

function ViewArticle() {
  // Route
  const { id = 0 } = useParamsEx({ id: 'number' });

  // Labels
  const labels = app.getLabels(
    'tab',
    'edit',
    'articleView',
    'slideshowLogo',
    'articleLogo'
  );

  React.useEffect(() => {
    // Page title
    app.setPageKey('articleView');
  }, []);

  return (
    <ViewPage<ArticleViewDto>
      fields={[
        { data: 'title', label: 'articleTitle' },
        { data: 'subtitle', label: 'articleSubtitle' },
        { data: 'tabName1', label: labels.tab },
        { data: 'tabName2', label: labels.tab + ' 2' },
        { data: 'tabName3', label: labels.tab + ' 3' },
        { data: 'url', label: 'articleUrl' },
        { data: 'description', label: 'articleDescription', singleRow: true },
        { data: 'keywords', label: 'articleKeywords', singleRow: true },
        { data: 'author', label: 'user', singleRow: 'small' },
        ['creation', GridDataType.DateTime, undefined, 'small'],
        {
          data: (item) =>
            DateUtils.sameDay(item.creation, item.release) ? (
              app.formatDate(item.release)
            ) : (
              <Typography color="red" variant="subtitle2">
                {app.formatDate(item.release)}
              </Typography>
            ),
          label: 'articleRelease',
          singleRow: 'small'
        },
        {
          data: (item) => (item.weight > 0 ? item.weight : undefined),
          label: 'articleWeight',
          singleRow: 'small'
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: 'status',
          singleRow: 'small'
        }
      ]}
      loadData={() => app.articleApi.viewRead(id)}
      actions={(data, refresh) => {
        const logoState = LocalUtils.createLogoState(data);
        return (
          <React.Fragment>
            <ButtonLink
              variant="outlined"
              href={`./../../logo/${data.id}`}
              startIcon={<PhotoIcon />}
              state={logoState}
            >
              {labels.articleLogo}
            </ButtonLink>
            <ButtonLink
              variant="outlined"
              href={`./../../gallery/${data.id}`}
              startIcon={<CollectionsIcon />}
              state={logoState}
            >
              {labels.slideshowLogo}
            </ButtonLink>
            <ButtonLink
              variant="outlined"
              href={`./../../edit/${data.id}`}
              startIcon={<EditIcon />}
            >
              {labels.edit}
            </ButtonLink>
          </React.Fragment>
        );
      }}
    >
      {(data) => (
        <iframe
          src={app.formatLink(data)}
          width="100%"
          height="480px"
          title={labels.articleView}
        />
      )}
    </ViewPage>
  );
}

export default ViewArticle;

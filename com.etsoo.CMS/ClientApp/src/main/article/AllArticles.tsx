import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  Tiplist
} from '@etsoo/materialui';
import { BoxProps, Fab, IconButton } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import PageviewIcon from '@mui/icons-material/Pageview';
import PublicIcon from '@mui/icons-material/Public';
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { DataTypes, DomUtils } from '@etsoo/shared';
import { app } from '../../app/MyApp';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';
import { ArticleQueryDto } from '../../dto/ArticleQueryDto';
import { UserRole } from '@etsoo/appscript';

function AllArticles() {
  // Route
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = DomUtils.dataAs(location.state, { id: 'string' });

  // Labels
  const labels = app.getLabels(
    'id',
    'creation',
    'actions',
    'add',
    'edit',
    'view',
    'articleTitle',
    'viewWebsite',
    'tab'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ArticleQueryDto>>();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey('articles');
  }, []);

  return (
    <ResponsivePage<ArticleQueryDto>
      mRef={ref}
      defaultOrderBy="creation"
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate(app.transformUrl('/home/article/add'))}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      }}
      fieldTemplate={{ tab: 'number', id: 'number' }}
      fields={[
        <SearchField label={labels.articleTitle} name="title" />,
        <Tiplist
          label={labels.tab}
          name="tab"
          search
          loadData={async (keyword, id) => {
            return await app.api.post<DataTypes.IdLabelItem[]>(
              'Tab/List',
              {
                id,
                keyword
              },
              { defaultValue: [], showLoading: false }
            );
          }}
        />,
        <SearchField label={labels.id} name="id" defaultValue={id} />
      ]}
      loadData={async (data) => {
        return await app.api.post<ArticleQueryDto[]>('Article/Query', data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: 'creation',
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          field: 'title',
          header: labels.articleTitle,
          sortable: false
        },
        {
          field: 'tabName1',
          header: labels.tab,
          width: 240,
          sortable: false
        },
        {
          width: 156,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ArticleQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <React.Fragment>
                {(adminPermission || data.isSelf) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`/home/article/edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`/home/article/view/${data.id}`}
                >
                  <PageviewIcon />
                </IconButtonLink>
                <IconButton
                  title={labels.viewWebsite}
                  target="_blank"
                  href={`${app.formatLink(data)}`}
                >
                  <PublicIcon />
                </IconButton>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[86, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, 'd'),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `/home/article/edit/${data.id}`
              },
              {
                label: labels.view,
                icon: <PageviewIcon />,
                action: `/home/article/view/${data.id}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllArticles;

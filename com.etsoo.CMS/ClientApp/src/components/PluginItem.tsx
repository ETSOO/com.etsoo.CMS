import { DomUtils, IActionResult, Utils } from '@etsoo/shared';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import HelpIcon from '@mui/icons-material/Help';
import { Box, Grid, IconButton, Paper, Typography } from '@mui/material';
import React from 'react';
import { app } from '../app/MyApp';
import { PluginDto } from '../api/dto/website/PluginDto';
import { PlugEditDto } from '../api/dto/website/PlugEditDto';
import { NotificationMessageType } from '@etsoo/react';

export function checkSecret(secret: string, minLength: number = 12) {
  if (
    (secret.startsWith('{') || secret.endsWith('}')) &&
    /"\s*:/.test(secret)
  ) {
    try {
      JSON.parse(secret);
      return true;
    } catch (e) {
      app.notifier.message(
        NotificationMessageType.Danger,
        `${e}`,
        'JSON secret error'
      );
      return false;
    }
  } else {
    return secret.length >= minLength;
  }
}

export async function loadPluginSecret(input: HTMLInputElement) {
  const id = input.id;
  if (id) {
    const data = await app.websiteApi.readService(input.id, {
      showLoading: false
    });
    if (data == null) return false;
    input.value = data.secret;
  }

  return false;
}

export interface PluginProps {
  /**
   * Plugin init items
   */
  initData?: PluginDto[];

  /**
   * Add/Edit button disabled
   */
  disabled?: boolean;
}

/**
 * Plugin item props
 */
export interface PluginItemProps {
  /**
   * Name
   */
  name: string;

  /**
   * Label
   */
  label?: string;

  /**
   * Plugin init data
   */
  initData?: PluginDto;

  /**
   * Inputs layout
   */
  inputs: (data?: PluginDto) => React.ReactNode;

  /**
   * URL of the product
   */
  url: string;

  /**
   * Add/Edit button disabled
   */
  disabled?: boolean;

  /**
   * Custom validator
   */
  validator?: (
    form: HTMLFormElement,
    data: PlugEditDto,
    editing: boolean
  ) => boolean | void;
}

/**
 * Plugin item
 * @param props Props
 * @returns Component
 */
export function PluginItem(props: PluginItemProps) {
  // Destruct
  const {
    name,
    label = app.get<string>(`service${name}`),
    initData,
    disabled,
    url,
    inputs,
    validator
  } = props;

  // State
  const [data, setData] = React.useState<PluginDto>();

  // Labels
  const labels = app.getLabels('add', 'edit', 'help', 'noChanges');

  const addService = (editing: boolean) => {
    app.showInputDialog({
      title: editing ? labels.edit : labels.add,
      message: label,
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        let {
          app: appId,
          secret,
          enabled = data?.enabled ?? false
        } = DomUtils.dataAs(new FormData(form), {
          app: 'string',
          secret: 'string',
          enabled: 'boolean'
        });

        if (appId == null) {
          DomUtils.setFocus('app', form);
          return false;
        }

        if (validator) {
          const appData = { app: appId, secret };
          if (validator(form, appData, editing) === false) {
            return false;
          } else {
            appId = appData.app;
            secret = appData.secret;
          }
        }

        // Request data
        const rq = {
          deviceId: app.deviceId,
          id: name,
          app: appId,
          enabled,
          secret: secret ? app.encrypt(secret) : undefined
        };

        let result: IActionResult | undefined;
        if (editing) {
          // Changed fields
          const fields: string[] = Utils.getDataChanges(
            { app: appId, secret, enabled },
            data!
          );
          if (fields.length === 0) {
            return labels.noChanges;
          }
          const editRQ = { ...rq, changedFields: fields };
          result = await app.websiteApi.updateService(editRQ, {
            showLoading: false
          });
        } else {
          result = await app.websiteApi.createService(rq, {
            showLoading: false
          });
        }

        if (result == null) return;

        if (result.ok) {
          var newData: PluginDto = {
            id: name,
            app: appId ?? data?.app,
            enabled: enabled,
            refreshTime: new Date()
          };
          setData(newData);

          return;
        }

        return app.formatResult(result);
      },
      inputs: inputs(data)
    });
  };

  React.useEffect(() => {
    setData(initData);
  }, [initData]);

  return (
    <Grid item xs={12} sm={6} lg={4}>
      <Paper
        sx={{
          padding: 2,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <Box flexGrow={2}>
          <Typography
            sx={{
              textDecoration:
                data == null || data.enabled ? 'unset' : 'line-through'
            }}
          >
            {label}
          </Typography>
          <Typography variant="caption">
            {name}
            {data?.refreshTime ? ', ' + app.formatDate(data.refreshTime) : ''}
          </Typography>
        </Box>
        <Box gap={1}>
          {data?.app ? (
            <IconButton
              title={labels.edit}
              color="primary"
              onClick={() => addService(true)}
              disabled={disabled}
            >
              <EditIcon />
            </IconButton>
          ) : (
            <IconButton
              title={labels.add}
              color="primary"
              onClick={() => addService(false)}
              disabled={disabled}
            >
              <AddIcon />
            </IconButton>
          )}
          <IconButton
            title={labels.help}
            size="small"
            href={url}
            target="_blank"
          >
            <HelpIcon />
          </IconButton>
        </Box>
      </Paper>
    </Grid>
  );
}

import { Switch, TextFieldEx, VBox } from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { app } from '../app/MyApp';
import { PluginItem, PluginProps } from './PluginItem';

/**
 * Next.js on-demand revalidation plugin
 * https://nextjs.org/docs/basic-features/data-fetching/incremental-static-regeneration
 * @returns Component
 */
export function NextJsPlugin(props: PluginProps) {
  const name = 'NextJs';
  const secret = '******';
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);

  const labels = app.getLabels(
    'serviceNextJsApp',
    'serviceNextJsToken',
    'enabled'
  );

  return (
    <PluginItem
      name={name}
      url="https://developers.google.com/recaptcha/docs/v3"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceNextJsApp}
            autoCorrect="off"
            defaultValue={data?.app ?? ''}
            type="url"
            showClear
            required
          />
          <TextFieldEx
            name="secret"
            label={labels.serviceNextJsToken}
            autoCorrect="off"
            defaultValue={data?.app ? secret : ''}
            showClear
            showPassword
            required
          />
          <Switch
            name="enabled"
            label={labels.enabled}
            checked={data?.enabled ?? true}
          />
        </VBox>
      )}
      validator={(form, data, editing) => {
        if (data.app.length < 16) {
          DomUtils.setFocus('app', form);
          return false;
        }

        if (
          data.secret == null ||
          (!editing && data.secret === secret) ||
          (data.secret !== secret && data.secret.length < 16)
        ) {
          DomUtils.setFocus('secret', form);
          return false;
        }

        if (editing && data.secret === secret) {
          data.secret = undefined;
        }
      }}
    />
  );
}

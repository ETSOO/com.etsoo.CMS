import { Switch, TextFieldEx, VBox } from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { app } from '../app/MyApp';
import { PluginItem, PluginProps } from './PluginItem';

/**
 * Google analytics
 * @returns Component
 */
export function GAPlugin(props: PluginProps) {
  const name = 'GA';
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);

  const labels = app.getLabels('serviceGAApp', 'enabled');

  return (
    <PluginItem
      name={name}
      url="https://analytics.google.com/analytics"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceGAApp}
            autoCorrect="off"
            defaultValue={data?.app ?? 'G-'}
            inputProps={{
              style: {
                textTransform: 'uppercase'
              }
            }}
            showClear
            required
          />
          <Switch
            name="enabled"
            label={labels.enabled}
            checked={data?.enabled ?? true}
          />
        </VBox>
      )}
      validator={(form, data, _editing) => {
        if (data.app.length < 12) {
          DomUtils.setFocus('app', form);
          return false;
        }
        data.app = data.app.toUpperCase();
      }}
    />
  );
}

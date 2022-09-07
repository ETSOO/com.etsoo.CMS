import { HiSelector, HiSelectorProps } from '@etsoo/materialui';
import { app } from '../app/MyApp';
import { TabDto } from '../dto/TabDto';
import { TabQueryRQ } from '../RQ/TabQueryRQ';

/**
 * Tab selector props
 */
export type TabSelectorProps = Omit<
  HiSelectorProps<TabDto>,
  'idField' | 'loadData' | 'labelField'
>;

/**
 * Tab selector
 * @param props Prop
 * @returns Component
 */
export function TabSelector(props: TabSelectorProps) {
  return (
    <HiSelector<TabDto>
      labelField="name"
      loadData={async (parent?: number) => {
        const rq: TabQueryRQ = { currentPage: 0, batchSize: 100, parent };
        return await app.api.post<TabDto[]>('Tab/Query', rq, {
          showLoading: false
        });
      }}
      {...props}
    />
  );
}

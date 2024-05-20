import { HiSelector, HiSelectorProps } from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { TabDto } from "../api/dto/tab/TabDto";

/**
 * Tab selector props
 */
export type TabSelectorProps = Omit<
  HiSelectorProps<TabDto>,
  "idField" | "loadData" | "labelField"
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
      loadData={(parent?: number) =>
        app.tabApi.query(
          { queryPaging: { currentPage: 0, batchSize: 100 }, parent },
          { showLoading: false }
        )
      }
      {...props}
    />
  );
}

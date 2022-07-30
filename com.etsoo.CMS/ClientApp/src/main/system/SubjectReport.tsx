import { CommonPage, useParamsEx } from '@etsoo/react';
import React from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { app } from '../../app/MyApp';
import { alpha, LinearProgress, Theme, useTheme } from '@mui/material';
import { BusinessUtils } from '@etsoo/appscript';

// https://www.chartjs.org/docs/latest/getting-started/
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface ReportResult {
  data1: { name: string };
  data2: ReportItem[];
}

interface ReportItem {
  rowIndex: number;
  d?: number;
  d0?: number;
}

function parseData(rawData: ReportItem[], theme: Theme) {
  // Start month
  const startMonth = new Date().getMonth() + 1;

  // Month labels
  const months = BusinessUtils.getMonths(
    app.get<string[]>('months') ?? [],
    startMonth
  );

  // Labels
  const labels = app.getLabels('subjectDataYTD', 'subjectDataMOM');

  // Datasets
  const datasets = [
    {
      label: labels.subjectDataYTD,
      data: rawData.map((d) => d.d),
      borderColor: theme.palette.primary.main,
      backgroundColor: theme.palette.primary.main
    },
    {
      label: labels.subjectDataMOM,
      data: rawData.map((d) => d.d0),
      borderColor: alpha(theme.palette.primary.main, 0.1),
      backgroundColor: alpha(theme.palette.primary.main, 0.1)
    }
  ];

  return {
    labels: months.map((m) => m.label),
    datasets
  };
}

function Reports() {
  // Route
  const { id: subjectId = 0 } = useParamsEx({ id: 'number' });

  // Theme
  const theme = useTheme();

  // State
  const [rawData, setRawData] = React.useState<ReportResult>();
  const isMounted = React.useRef(true);

  React.useEffect(() => {
    // Page title
    app.setPageKey('reports');

    // Load data
    app.serviceApi
      .get<ReportResult>(`AccountLine/ReportSubject/${subjectId}`)
      .then((rawData) => {
        if (rawData == null) return;
        setRawData(rawData);
      });

    return () => {
      isMounted.current = false;
      app.pageExit();
    };
  }, [subjectId]);

  return (
    <CommonPage>
      {rawData == null ? (
        <LinearProgress />
      ) : (
        <Line
          options={{
            responsive: true,
            plugins: {
              legend: {
                position: 'top' as const
              },
              title: {
                display: true,
                text: rawData.data1.name
              }
            }
          }}
          data={parseData(rawData.data2, theme)}
        />
      )}
    </CommonPage>
  );
}

export default Reports;

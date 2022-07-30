import {
  globalApp,
  HBox,
  IconButtonLink,
  ListMoreDisplay,
  ListMoreDisplayProps,
  MUGlobal,
  SearchBar,
  SearchField
} from '@etsoo/react';
import { DateUtils, Utils } from '@etsoo/shared';
import { Box, Theme, Typography, useTheme } from '@mui/material';
import React from 'react';
import PageviewIcon from '@mui/icons-material/Pageview';

/**
 * History display line
 */
export interface HistoryLine {
  id: number;
  subject: string;
  isIncome: boolean;
  user: string;
  title: string;
  amount: number;
  happenDate: Date;
}

/**
 * History account
 */
export interface HistoryAccount {
  externalAccount: boolean;
  accountBank: string;
  accountName: string;
  accountNumber: string;
}

/**
 * History subject
 */
export interface HistorySubject {
  name: string;
  isIncome: boolean;
}

/**
 * History display props
 */
export interface HistoryDisplayProps
  extends Omit<ListMoreDisplayProps<HistoryLine>, 'children'> {
  formatAmount: (data: HistoryLine) => [boolean, string?];
}

// Get label
const getLabel = (key: string) => {
  if (typeof globalApp === 'undefined') return key;
  return globalApp.get(Utils.formatInitial(key)) ?? key;
};

const getItemStyle = (index: number, theme: Theme) => {
  return {
    padding: [theme.spacing(1.5), theme.spacing(1)].join(' '),
    background:
      index % 2 === 0 ? theme.palette.grey[100] : theme.palette.grey[50]
  };
};

const itemRenderer = (
  data: HistoryLine,
  _index: number,
  formatAmount: (data: HistoryLine) => [boolean, string?]
) => {
  const [p, amount] = formatAmount(data);

  return (
    <HBox display="flex" justifyContent="space-between">
      <Typography>
        {globalApp.formatDate(data.happenDate)}, {data.user}, {data.subject} -{' '}
        {data.title}
        <IconButtonLink
          title={getLabel('view')}
          href={`/home/account/lines/view/${data.id}`}
        >
          <PageviewIcon />
        </IconButtonLink>
      </Typography>
      <Typography
        sx={{
          width: '108px',
          flexGrow: 0,
          flexShrink: 0,
          textAlign: 'right',
          color: (theme) => (p ? undefined : theme.palette.error.main)
        }}
      >
        {amount}
      </Typography>
    </HBox>
  );
};

/**
 * History display
 * @param props Props
 * @returns Component
 */
export function HistoryDisplay(props: HistoryDisplayProps) {
  // Destruct
  const happenDateEndRef = React.useRef<HTMLInputElement>();
  const {
    formatAmount,
    headerRenderer = (reset) => (
      <Box
        sx={{ height: 40, marginBottom: MUGlobal.half(MUGlobal.pagePaddings) }}
      >
        <SearchBar
          fields={[
            <SearchField label={getLabel('title')} name="title" />,
            <SearchField
              label={getLabel('happenDate')}
              name="happenDateStart"
              type="date"
              onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                if (happenDateEndRef.current == null) return;
                happenDateEndRef.current.min = DateUtils.formatForInput(
                  event.currentTarget.valueAsDate
                );
              }}
              inputProps={{ max: DateUtils.formatForInput() }}
            />,
            <SearchField
              label=""
              name="happenDateEnd"
              type="date"
              inputRef={happenDateEndRef}
              inputProps={{
                max: DateUtils.formatForInput()
              }}
            />
          ]}
          onSubmit={(form) => reset(form)}
        />
      </Box>
    ),
    ...rest
  } = props;

  // Theme
  const theme = useTheme();

  // Layout
  return (
    <ListMoreDisplay headerRenderer={headerRenderer} {...rest}>
      {(data, index) => (
        <div key={data.id} style={getItemStyle(index, theme)}>
          {itemRenderer(data, index, formatAmount)}
        </div>
      )}
    </ListMoreDisplay>
  );
}

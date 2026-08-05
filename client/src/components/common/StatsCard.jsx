import React from 'react';
import { Card, CardContent, Box, Typography, Avatar } from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import RemoveIcon from '@mui/icons-material/Remove';

const colorMap = {
  primary: { bg: '#E3F2FD', color: '#1565C0', icon: '#1565C0' },
  secondary: { bg: '#F3E5F5', color: '#7B1FA2', icon: '#7B1FA2' },
  success: { bg: '#E8F5E9', color: '#2E7D32', icon: '#2E7D32' },
  error: { bg: '#FFEBEE', color: '#D32F2F', icon: '#D32F2F' },
  warning: { bg: '#FFF3E0', color: '#F57C00', icon: '#F57C00' },
  info: { bg: '#E1F5FE', color: '#0288D1', icon: '#0288D1' },
};

function TrendIcon({ trend }) {
  if (trend === 'up') return <TrendingUpIcon sx={{ fontSize: 16, color: 'success.main' }} />;
  if (trend === 'down') return <TrendingDownIcon sx={{ fontSize: 16, color: 'error.main' }} />;
  return <RemoveIcon sx={{ fontSize: 16, color: 'text.secondary' }} />;
}

export default function StatsCard({
  icon,
  title,
  value,
  trend,
  trendValue,
  color = 'primary',
}) {
  const palette = colorMap[color] || colorMap.primary;

  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: 2.5 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box sx={{ flex: 1 }}>
            <Typography
              variant="body2"
              color="text.secondary"
              fontWeight={500}
              sx={{ mb: 1, textTransform: 'uppercase', letterSpacing: 0.5 }}
            >
              {title}
            </Typography>
            <Typography variant="h4" fontWeight={700} sx={{ mb: 1 }}>
              {value}
            </Typography>
            {(trend || trendValue) && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <TrendIcon trend={trend} />
                {trendValue && (
                  <Typography
                    variant="body2"
                    color={trend === 'up' ? 'success.main' : trend === 'down' ? 'error.main' : 'text.secondary'}
                    fontWeight={600}
                  >
                    {trendValue}
                  </Typography>
                )}
              </Box>
            )}
          </Box>

          {icon && (
            <Avatar
              sx={{
                width: 48,
                height: 48,
                bgcolor: palette.bg,
                color: palette.color,
              }}
            >
              {icon}
            </Avatar>
          )}
        </Box>
      </CardContent>
    </Card>
  );
}

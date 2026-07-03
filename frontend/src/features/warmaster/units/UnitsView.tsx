import { Alert, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';
import { useEffect } from 'react';
import type { WarmasterUnit } from '../../../types/warmaster';
import { useWarmasterList } from '../useWarmasterList';

type UnitsViewProps = {
  token: string;
};

export function UnitsView({ token }: UnitsViewProps) {
  const { items, loadItems, message } = useWarmasterList<WarmasterUnit>(token, '/api/warmaster/units');

  useEffect(() => {
    void loadItems();
  }, [loadItems]);

  return (
    <>
      {message && <Alert severity="error">{message}</Alert>}
      <TableContainer component={Paper} variant="outlined">
        <Table aria-label="Units table">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((unit) => (
              <TableRow key={unit.id}>
                <TableCell>{unit.name}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  );
}

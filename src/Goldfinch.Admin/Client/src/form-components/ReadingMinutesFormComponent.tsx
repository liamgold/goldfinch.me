import React, { useState } from 'react';
import { type FormComponentProps, useFormComponentCommandProvider } from '@kentico/xperience-admin-base';
import { Button, ButtonColor, FormEditMode, FormItemWrapper, Input } from '@kentico/xperience-admin-components';

const REGENERATE_COMMAND_NAME = 'Regenerate';

/**
 * Registered under ClientComponentName "@goldfinch/web-admin/ReadingMinutes".
 * Kentico automatically appends "FormComponent" when resolving the export.
 */
export const ReadingMinutesFormComponent = (props: FormComponentProps<number>) => {
  const { executeCommand } = useFormComponentCommandProvider();
  const [isRegenerating, setIsRegenerating] = useState(false);

  const isDisabled = props.editMode === FormEditMode.Disabled || props.editMode === FormEditMode.ReadOnly;

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const parsed = Number(event.target.value);
    props.onChange?.(Number.isFinite(parsed) ? parsed : 0);
  };

  const handleRegenerate = async () => {
    setIsRegenerating(true);
    try {
      const minutes = await executeCommand<number>(props, REGENERATE_COMMAND_NAME);
      if (typeof minutes === 'number') {
        props.onChange?.(minutes);
      }
    } finally {
      setIsRegenerating(false);
    }
  };

  return (
    <FormItemWrapper
      id={props.name}
      label={props.label}
      markAsRequired={props.required}
      editMode={props.editMode}
      invalid={props.invalid}
      validationMessage={props.validationMessage}
      statusText={props.statusText}
      explanationText={props.explanationText}
    >
      <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
        <Input
          id={props.name}
          type="number"
          value={(props.value ?? 0).toString()}
          onChange={handleChange}
          disabled={isDisabled}
        />
        <Button
          label="Regenerate"
          color={ButtonColor.Secondary}
          inProgress={isRegenerating}
          disabled={isDisabled}
          onClick={handleRegenerate}
        />
      </div>
    </FormItemWrapper>
  );
};
